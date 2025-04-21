#nullable enable
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
    const String REG_CERTSRV_TEMPLATE = @"SYSTEM\CurrentControlSet\Services\CertSvc\Configuration\{0}\ExitModules\{1}";
    const String REG_NDES_TEMPLATE = @"Software\Microsoft\Cryptography\MSCEP";

    /// <summary>
    /// Initializes a new instance of <strong>RegistryService</strong>
    /// </summary>
    /// <param name="moduleName">Module ProgID.</param>
    /// <param name="policy">A boolean value that indicates whether the module is policy or exit module.</param>
    /// <exception cref="ArgumentException"></exception>
    [Obsolete("Use an overload that accepts 'CertServerModuleType' parameter.")]
    protected RegistryService(String moduleName, Boolean policy = false) : this(moduleName, policy ? CertServerModuleType.Policy : CertServerModuleType.Exit) { }

    /// <summary>
    /// Initializes a new instance of <strong>RegistryService</strong>
    /// </summary>
    /// <param name="moduleName">Module's COM ProgID.</param>
    /// <param name="moduleType">Module type.</param>
    /// <exception cref="ArgumentException">CA name could not be determined.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Module type is not valid module type.</exception>
    protected RegistryService(String moduleName, CertServerModuleType moduleType) {
        String regPath;
        if (String.IsNullOrEmpty(moduleName)) {
            throw new ArgumentException("Registry key base name cannot be empty.");
        }
        
        RegPath = String.Empty;

        String moduleName1 = moduleName;
        switch (moduleType) {
            case CertServerModuleType.Policy:
                regPath = REG_CERTSRV_TEMPLATE.Replace("ExitModules", "PolicyModules");
                break;
            case CertServerModuleType.Exit:
                regPath = REG_CERTSRV_TEMPLATE;
                break;
            case CertServerModuleType.NDES:
                regPath = REG_NDES_TEMPLATE;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(moduleType), moduleType, null);
        }

        switch (moduleType) {
            case CertServerModuleType.Policy:
            case CertServerModuleType.Exit:
                if (GetRecord("Active", @"SYSTEM\CurrentControlSet\Services\CertSvc\Configuration")?.Value is not String activeCA) {
                    throw new ArgumentException("CA name could not be determined.");
                }
                RegPath = String.Format(regPath, activeCA, moduleName1);
                String baseKey = String.Format(regPath, activeCA, String.Empty);
                if (!RegKeyExists(RegPath)) {
                    using RegistryKey? key = Registry.LocalMachine.OpenSubKey(baseKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    key?.CreateSubKey(moduleName1);
                }
                break;
            case CertServerModuleType.NDES:
                RegPath = REG_NDES_TEMPLATE;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    /// <summary>
    /// This property is obsolete and returns null.
    /// </summary>
    [Obsolete("This member is obsolete.", true)]
    protected String? Config => null;
    /// <summary>
    /// Gets the module's registry home key.
    /// </summary>
    protected String RegPath { get; }

    /// <summary>
    /// Checks if registry key exists in registry.
    /// </summary>
    /// <param name="regKey">Registry key full path.</param>
    /// <returns><strong>True</strong> if registry key exists, otherwise <strong>False</strong>.</returns>
    protected static Boolean RegKeyExists(String regKey) {
        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(regKey);
        return key is not null;
    }

    /// <summary>
    /// This method is obsolete.
    /// </summary>
    /// <param name="bstrConfig">This parameter is ignored.</param>
    [Obsolete("This method is obsolete and does nothing.")]
    protected void Initialize(String? bstrConfig = null) { }

    /// <summary>
    /// Gets referral record from registry.
    /// </summary>
    /// <param name="target">
    /// Colon-separated string with exactly 3 tokens: {hive}:{RegistryKey}:{ValueName}
    /// </param>
    /// <returns>Referral registry entry if found, otherwise null.</returns>
    protected static RegTriplet? GetReferralRecord(String target) {
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

        RegTriplet? retValue;
        using (RegistryKey? key = hive.OpenSubKey(tokens[1])) {
            if (key is null) {
                hive.Close();
                return null;
            }
            if (key.GetValueNames().FirstOrDefault(x => x.Equals(tokens[2], StringComparison.InvariantCultureIgnoreCase)) is null) {
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
    protected RegTriplet? GetRecord(String name, String? path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path);
        if (key is null) {
            return null;
        }

        IDictionary<String, String> valueNames = key.GetValueNames().ToDictionary(x => x, StringComparer.OrdinalIgnoreCase);

        if (valueNames.Count == 0 || !valueNames.ContainsKey(name)) {
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
    protected IEnumerable<RegTriplet> GetRecords(String? path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path);
        var values = new List<RegTriplet>();
        if (key is not null) {
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
    protected void WriteRecord(RegTriplet valuePair, String? path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key is not null) {
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
    protected void WriteRecords(IEnumerable<RegTriplet> valuePair, String? path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key is not null) {
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
    protected void DeleteRecord(String name, String? path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        key?.DeleteValue(name);
    }
    /// <summary>
    /// Deletes named registry entries from the current module's configuration storage.
    /// </summary>
    /// <param name="names">A collection of registry value names.</param>
    /// <param name="path">Additional path below module's configuration storage.</param>
    protected void DeleteRecords(IEnumerable<String> names, String? path = null) {
        if (String.IsNullOrWhiteSpace(path)) {
            path = RegPath;
        }

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key is not null) {
            foreach (String name in names) {
                key.DeleteValue(name);
            }
        }
    }
}