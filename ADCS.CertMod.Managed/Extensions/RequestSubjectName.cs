namespace ADCS.CertMod.Managed.Extensions;

/// <summary>
/// Contains enumeration of subject/issuer name RDN attributes.
/// </summary>
enum RequestSubjectName {
    DistinguishedName,
    RawName,
    Country,
    Organization,
    OrgUnit,
    CommonName,
    Locality,
    State,
    Title,
    GivenName,
    Initials,
    SurName,
    DomainComponent,
    EMail,
    StreetAddress,
    UnstructuredName,
    UnstructuredAddress,
    DeviceSerialNumber
}