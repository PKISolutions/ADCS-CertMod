using CERTCLILib;

namespace ADCS.CertMod.Managed.Exit;

class CertServerExitManaged : CertServerExitPolicyManaged {
    public CertServerExitManaged() : base(new CCertServerExitClass()) { }
}