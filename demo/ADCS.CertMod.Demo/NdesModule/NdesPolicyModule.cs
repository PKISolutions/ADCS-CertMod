using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ADCS.CertMod.Managed;
using ADCS.CertMod.Managed.NDES;
using CERTENROLLLib;

namespace ADCS.CertMod.Demo.NdesModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("AdcsCertMod_Demo.Ndes")]
[Guid("246B051D-B7AA-463A-8A7D-D93817A16759")]
public class NdesPolicyModule : NdesPolicyBase {
    public NdesPolicyModule() : base(new LogWriter("Demo.Ndes", LogLevel.Trace, @"C:\"), new DefaultSCEPChallengeStore(new DefaultSCEPChallengeGenerator())) { }
    protected override Boolean OnVerifyRequest(Byte[]? pkcs10Request, X509Certificate2? signingCertificate, String template, String transactionID) {
        // this condition is only a safeguard. Not intended to succeed in normal conditions.
        if (pkcs10Request is null) {
            Logger.LogError("Request is null.");
            return false;
        }
        if (signingCertificate is null) {
            Logger.LogError("Cert is null.");
        } else {
            Logger.LogError("Cert is not null");
        }

        // convert binary request to base64 string
        String b64 = Convert.ToBase64String(pkcs10Request);
        try {
            // decode request using IX509CertificateRequestPkcs10
            var req = new CX509CertificateRequestPkcs10();
            req.InitializeDecode(b64);
            // if signing certificate is null, then it is initial request and MUST include challenge password attribute
            if (signingCertificate is null) {
                // try to read challenge password attribute. It's always there. NDES will not invoke OnVerifyRequest
                // method when request is initial and don't have challenge password attribute.
                ICryptAttribute? pwdAttr = req.CryptAttributes.Cast<ICryptAttribute>().FirstOrDefault(x => x.ObjectId.Value == "1.2.840.113549.1.9.7");
                // but anyway, safeguard check.
                if (pwdAttr is null) {
                    Logger.LogError("Challenge password attribute not found.");
                    return false;
                }

                String challengePwdFromReqB64 = pwdAttr.Values[0].RawData[EncodingType.XCN_CRYPT_STRING_BASE64]!;
                // attribute value is ASN.1-encoded printable string. So we strip ASN header, which is 2 bytes (when payload is less than 128 bytes)
                Byte[] challengePwdBytes = Convert.FromBase64String(challengePwdFromReqB64).Skip(2).ToArray();
                String challengePwdFromReq = Encoding.UTF8.GetString(challengePwdBytes);
                Logger.LogDebug("Challenge password from request: " + challengePwdFromReq);
                if (!ChallengeStore.TryGetChallenge(challengePwdFromReq, out SCEPChallengeStoreEntry? entry)) {
                    Logger.LogError("Specified challenge password not found in store.");
                    return false;
                }
                // put extra logic associated with 'entry' variable if needed. Maybe log in database.
                Logger.LogDebug("""
                            Challenge properties:
                                password: {0}
                                template: {1}
                                params  : {2}
                            """, entry!.Challenge, entry.Template, entry.Parameters);

                Logger.LogDebug("Looks ok.");
            } else {
                // apply optional checks for renewal request if needed
            }
        } catch (Exception ex) {
            Logger.LogError(ex, "OnVerifyRequest");
            return false;
        }
        return true;
    }
}
