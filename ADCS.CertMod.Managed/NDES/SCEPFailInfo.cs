namespace ADCS.CertMod.Managed.NDES;

enum SCEPFailInfo {
    SCEPFailUnknown         = -1,
    SCEPFailBadAlgorithm    = 0,
    SCEPFailBadMessageCheck = 1,
    SCEPFailBadRequest      = 2,
    SCEPFailBadTime         = 3,
    SCEPFailBadCertId       = 4
}