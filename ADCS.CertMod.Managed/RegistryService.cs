using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using ADCS.CertMod.Managed.Extensions;
using Microsoft.Win32;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents registry access utility service that simplifies registry access by policy or exit module.
/// This class is abstract and cannot be instantiated directly.
/// </summary>
public abstract class RegistryService {
    const String REG_TEMPLATE = @"SYSTEM\CurrentControlSet\Services\CertSvc\Configuration\{0}\ExitModules\{1}";
    readonly String _moduleName, _regPath;

    Boolean isInitialized;

    /// <summary>
    /// Initializes a new instance of <strong>RegistryService</strong>
    /// </summary>
    /// <param name="moduleName">Module ProgID.</param>
    /// <param name="policy">A boolean value that indicates whether the module is policy or exit module.</param>
    /// <exception cref="ArgumentException"></exception>
    protected RegistryService(String moduleName, Boolean policy = false) {
        if (String.IsNullOrEmpty(moduleName)) {
            throw new ArgumentException("Registry key base name cannot be empty.");
        }

        _regPath = policy
            ? REG_TEMPLATE.Replace("ExitModules", "PolicyModules")
            : REG_TEMPLATE;

        _moduleName = moduleName;
    }

    /// <summary>
    /// Gets the ADCS Certification Authority config string.
    /// </summary>
    protected String Config { get; private set; }
    /// <summary>
    /// Gets the Exit or Policy Module's registry home key.
    /// </summary>
    protected String RegPath { get; private set; }

    /// <summary>
    /// Checks if registry key exists in registry.
    /// </summary>
    /// <param name="regKey">Registry key full path.</param>
    /// <returns><strong>True</strong> if registry key exists, otherwise <strong>False</strong>.</returns>
    protected static Boolean RegKeyExists(String regKey) {
        using RegistryKey key = Registry.LocalMachine.OpenSubKey(regKey);
        return key != null;
    }

    /// <summary>
    /// Initializes registry service. When overriden, base method must be called first.
    /// </summary>
    /// <param name="bstrConfig">Specifies the configuration name retrieved from ADCS Exit Module initialization.</param>
    protected void Initialize(String bstrConfig) {
        if (isInitialized) {
            return;
        }

        if (bstrConfig.Contains("\\")) {
            String[] tokens = bstrConfig.Split('\\');
            Config = tokens[1];
        } else {
            Config = bstrConfig;
        }

        RegPath = String.Format(_regPath, Config, _moduleName);

        String baseKey = String.Format(_regPath, Config, String.Empty);
        if (!RegKeyExists(RegPath)) {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(baseKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
            key?.CreateSubKey(_moduleName);
        }

        isInitialized = true;
    }

    /// <summary>
    /// Gets referral record from registry.
    /// </summary>
    /// <param name="target">
    /// Colon-separated string with exactly 3 tokens: {hive}:{RegistryKey}:{ValueName}
    /// </param>
    /// <returns>Referral registry entry if found, otherwise null.</returns>
    protected static RegTriplet GetReferralRecord(String target) {
        String[] tokens = target.Split(':');
        if (tokens.Length != 3) {
            return null;
        }
        RegistryKey hive;
        switch (tokens[0].ToLower()) {
            case "hkcr":
                hive = Registry.ClassesRoot;
                break;
            case "hkcu":
                hive = Registry.CurrentUser;
                break;
            case "hklm":
                hive = Registry.LocalMachine;
                break;
            case "hku":
                hive = Registry.Users;
                break;
            default:
                hive = Registry.LocalMachine;
                break;
        }

        RegTriplet retValue;
        using (RegistryKey key = hive.OpenSubKey(tokens[1])) {
            if (key == null) {
                hive.Close();
                return null;
            }
            if (key.GetValueNames().FirstOrDefault(x => x.Equals(tokens[2], StringComparison.InvariantCultureIgnoreCase)) == null) {
                hive.Close();
                return null;
            }
            retValue = new RegTriplet(tokens[2], key.GetValueKind(tokens[2])) { Value = key.GetValue(tokens[2]) };
        }
        hive.Close();

        return retValue;
    }
    /// <summary>
    /// Gets named record from the current module's configuration storage.
    /// </summary>
    /// <param name="name">Registry value name to read.</param>
    /// <param name="path">Additional path below module's configuration storage.</param>
    /// <returns>Requested registry value if present, or null if no matching entry found.</returns>
    protected RegTriplet GetRecord(String name, String path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
        if (key == null) {
            return null;
        }

        IDictionary<String, String> valueNames = key.GetValueNames().ToDictionary(x => x.ToLower());

        if (valueNames.Count == 0 || !valueNames.ContainsKey(name.ToLower())) {
            return null;
        }

        var retValue = new RegTriplet(name, key.GetValueKind(name)) { Value = key.GetValue(name) };

        if (retValue.Type == RegistryValueKind.String && ((String)retValue.Value).Split(':').Length == 3) {
            retValue = GetReferralRecord((String)retValue.Value);
        }

        return retValue;
    }
    /// <summary>
    /// Gets all records from the current module's configuration storage.
    /// </summary>
    /// <param name="path">Additional path below module's configuration storage.</param>
    /// <returns>A collection of registry entries.</returns>
    protected IEnumerable<RegTriplet> GetRecords(String path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
        var values = new List<RegTriplet>();
        if (key != null) {
            String[] valueNames = key.GetValueNames();
            if (valueNames.Length == 0) {
                return null;
            }
            values.AddRange(valueNames.Select(val => new RegTriplet(val, key.GetValueKind(val)) { Value = key.GetValue(val) }));
            return values;
        }

        return null;
    }
    /// <summary>
    /// Creates or updates registry entry under current module's configuration storage.
    /// </summary>
    /// <param name="valuePair">Registry entry to write.</param>
    /// <param name="path">Additional path below module's configuration storage.</param>
    protected void WriteRecord(RegTriplet valuePair, String path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key != null) {
            if (valuePair.Value is SecureString ss) {
                valuePair.Value = ss.EncryptPassword();
            }
            key.SetValue(valuePair.Name, valuePair.Value, valuePair.Type);
        }
    }
    /// <summary>
    /// Creates or updates registry entries under current module's configuration storage.
    /// </summary>
    /// <param name="valuePair">A collection of entries to write.</param>
    /// <param name="path">Additional path below module's configuration storage.</param>
    protected void WriteRecords(IEnumerable<RegTriplet> valuePair, String path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key != null) {
            foreach (RegTriplet entry in valuePair) {
                if (entry.Value is SecureString ss) {
                    entry.Value = ss.EncryptPassword();
                }
                key.SetValue(entry.Name, entry.Value, entry.Type);
            }
        }
    }
    /// <summary>
    /// Deletes named registry entry from the current module's configuration storage.
    /// </summary>
    /// <param name="name">Registry value name.</param>
    /// <param name="path">Additional path below module's configuration storage.</param>
    protected void DeleteRecord(String name, String path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        key?.DeleteValue(name);
    }
    /// <summary>
    /// Deletes named registry entries from the current module's configuration storage.
    /// </summary>
    /// <param name="names">A collection of registry value names.</param>
    /// <param name="path">Additional path below module's configuration storage.</param>
    protected void DeleteRecords(IEnumerable<String> names, String path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key != null) {
            foreach (String name in names) {
                key.DeleteValue(name);
            }
        }
    }
}