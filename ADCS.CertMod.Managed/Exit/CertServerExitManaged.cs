using ADCS.CertMod.Managed.Interop;

namespace ADCS.CertMod.Managed.Exit;

class CertServerExitManaged() : CertServerExitPolicyManaged(CertServerComFactory.CreateCertServerExit());