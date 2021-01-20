$mycert = New-SelfSignedCertificate -Subject "Smart-Clik" -Type CodeSigningCert -CertStoreLocation "Cert:\LocalMachine\my"
$mycert
