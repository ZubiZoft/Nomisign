using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Services
{
    public class ConstanciaNOM151
    {
        public string constancia;
        public string descripcion;
        public string folio;
        public string estado;

        public ConstanciaNOM151(string constancia, string descripcion, string folio, string estado)
        {
            this.constancia = constancia;
            this.descripcion = descripcion;
            this.folio = folio;
            this.estado = estado;
        }
    }
}