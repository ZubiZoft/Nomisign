using CfdiService.Model;
using Com.Advantage.Nom151;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace CfdiService.Services
{
    public class Nom151Service
    {
        private static readonly string _pfxFileName = @"C:\inetpub\wwwroot\nomiadmin\nomisign.pfx";
        private static readonly string _pfxFilePassword = "N0M1secure750";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string CreateNom151(string pdfPath, Document document)
        {
            try
            {
                FileInfo fileArp1 = new FileInfo(pdfPath);
                String expCert = _pfxFileName;
                String expCertPwd = _pfxFilePassword;

                //StreamReader aIn = null;

                //Generacion del Archivo Parcial 1
                ArchivoParcial arp1 = new ArchivoParcial(fileArp1);
                arp1.GetEncoded();
                //Generacion del Expediente
                //Indice Archivo Parcial 1
                NombreOP arpar1Nombre = new NombreOP(document.DocumentId.ToString());
                ResumenOP arpar1Resumen = new ResumenOP();
                arpar1Resumen.setAlgoritmoResumen(new AlgorithmIdentifier(ASN1Helper.SHA256));
                DerBitString DERBitString = new DerBitString(Encoding.ASCII.GetBytes(ASN1Helper.SHA256_ALGORITHM), arp1.GetEncoded().Length);
                arpar1Resumen.setResumen(DERBitString);
                EntradaIndice entIndice1 = new EntradaIndice();
                entIndice1.setNombre(arpar1Nombre);
                entIndice1.setResumen(arpar1Resumen);

                //Collection indice = new HashSet();
                ArrayList indice = new ArrayList();
                indice.Add(entIndice1);

                IdentificadorUsuario idUsr = new IdentificadorUsuario();
                NombreRazonSocialIdU razonSocial = new NombreRazonSocialIdU();

                String TIPO_PERSONA = "Persona Moral";
                if (TIPO_PERSONA.Equals("Persona Moral"))
                {
                    DerObjectIdentifier DERObjectIdentifier = new DerObjectIdentifier(ASN1Helper.NOM_I_PERSONA_MORAL);
                    idUsr.setPersonaFisicaMoral(DERObjectIdentifier);
                    DerUtf8String DERUTF8String = new DerUtf8String("Requordit.MX SA de CV");
                    razonSocial.setIdPersonaMoral(DERUTF8String);
                }
                else
                {
                    NombrePersonaFisica personaFisica = new NombrePersonaFisica();
                    personaFisica.setNombre("Maria Estela Gonzalez Villaseñor");
                    DerObjectIdentifier DERObjectIdentifier = new DerObjectIdentifier(ASN1Helper.NOM_I_PERSONA_FISICA);
                    idUsr.setPersonaFisicaMoral(DERObjectIdentifier);
                    razonSocial.setIdPersonaFisica(personaFisica);
                }
                idUsr.setIdRazonSocial(razonSocial);

                string TIPO_ID = "Cedula Fiscal";
                switch (TIPO_ID)
                {
                    case "Nombre Fiscal":
                        DerObjectIdentifier DERObjectIdentifier = new DerObjectIdentifier(ASN1Helper.NOM_IM_NOMBRE);
                        idUsr.setTipoIdU(DERObjectIdentifier);
                        break;
                    case "Nombre":
                        DerObjectIdentifier DERObjectIdentifier_2 = new DerObjectIdentifier(ASN1Helper.NOM_IF_NOMBRE);
                        idUsr.setTipoIdU(DERObjectIdentifier_2);
                        break;
                    case "Cedula Fiscal":
                        DerObjectIdentifier DERObjectIdentifier_3 = new DerObjectIdentifier(TIPO_PERSONA.Equals("Persona Moral") ? ASN1Helper.NOM_IM_CEDULAFISCAL : ASN1Helper.NOM_IF_CEDULAFISCAL);
                        idUsr.setTipoIdU(DERObjectIdentifier_3);
                        break;
                    case "CURP":
                        DerObjectIdentifier DERObjectIdentifier_4 = new DerObjectIdentifier(TIPO_PERSONA.Equals("Persona Moral") ? ASN1Helper.NOM_IM_CURP : ASN1Helper.NOM_IF_CURP);
                        idUsr.setTipoIdU(DERObjectIdentifier_4);
                        break;
                    case "IFE":
                        DerObjectIdentifier DERObjectIdentifier_5 = new DerObjectIdentifier(ASN1Helper.NOM_IF_IFE);
                        idUsr.setTipoIdU(DERObjectIdentifier_5);
                        break;
                    case "Pasaporte":
                        DerObjectIdentifier DERObjectIdentifier_6 = new DerObjectIdentifier(ASN1Helper.NOM_IF_PASAPORTE);
                        idUsr.setTipoIdU(DERObjectIdentifier_6);
                        break;
                }
                string CONTENIDO_ID = "REQ1409222L6";
                DerUtf8String DERUtf8String = new DerUtf8String(CONTENIDO_ID);
                idUsr.setContenidoIdU(DERUtf8String);

                bool EXISTE_REPRESENTANTE_LEGAL = true;
                string REPRESENTANTE_LEGAL_TIPO_ID = "IFE";
                string REPRESENTANTE_LEGAL_ID = "1644249848";
                if (EXISTE_REPRESENTANTE_LEGAL)
                {
                    IdentificadorPersona idRepresentante = new IdentificadorPersona();
                    NombreRazonSocialIdU representante = new NombreRazonSocialIdU();
                    NombrePersonaFisica personaFisica = new NombrePersonaFisica();
                    personaFisica.setNombre("Maria Estela");
                    personaFisica.setApellido1("Gonzalez");
                    personaFisica.setApellido2("Villaseñor");
                    representante.setIdPersonaFisica(personaFisica);
                    idRepresentante.setNombre(representante);

                    switch (REPRESENTANTE_LEGAL_TIPO_ID)
                    {
                        case "Nombre":
                            DerObjectIdentifier DERObjectIdentifier = new DerObjectIdentifier(ASN1Helper.NOM_IF_NOMBRE);
                            idRepresentante.setTipo(DERObjectIdentifier);
                            break;
                        case "Cedula Fiscal":
                            DerObjectIdentifier DERObjectIdentifier_2 = new DerObjectIdentifier(ASN1Helper.NOM_IF_CEDULAFISCAL);
                            idRepresentante.setTipo(DERObjectIdentifier_2);
                            break;
                        case "CURP":
                            DerObjectIdentifier DERObjectIdentifier_4 = new DerObjectIdentifier(ASN1Helper.NOM_IF_CURP);
                            idRepresentante.setTipo(DERObjectIdentifier_4);
                            break;
                        case "IFE":
                            DerObjectIdentifier DERObjectIdentifier_5 = new DerObjectIdentifier(ASN1Helper.NOM_IF_IFE);
                            idRepresentante.setTipo(DERObjectIdentifier_5);
                            break;
                        case "Pasaporte":
                            DerObjectIdentifier DERObjectIdentifier_6 = new DerObjectIdentifier(ASN1Helper.NOM_IF_PASAPORTE);
                            idRepresentante.setTipo(DERObjectIdentifier_6);
                            break;
                    }
                    DerUtf8String DERUtf8String_2 = new DerUtf8String(REPRESENTANTE_LEGAL_ID);
                    idRepresentante.setContenido(DERUtf8String_2);
                    idUsr.setRepresentanteIdU(idRepresentante);

                }

                IDUsuarioOP idUsuario = new IDUsuarioOP(idUsr);

                Expediente exp = new Expediente();
                exp.setNombreExpedienteToString(document.PathToFile);
                exp.setIndice(indice);
                exp.setIdUsuario(idUsuario);

                exp.sign(ASN1Helper.SHA256_WITH_RSA_ENCRIPTION, 1, expCert, expCertPwd);
                string texre = Convert.ToBase64String(exp.GetEncoded());
                return texre;
            }
            catch (Exception ex)
            {
                log.Info(ex);
            }
            return null;
        }
    }
}