$mycert = New-SelfSignedCertificate -Subject "Meetag" -Type CodeSigningCert -CertStoreLocation "Cert:\LocalMachine\my"
$mycert
