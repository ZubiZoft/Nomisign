using CfdiService.Model;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class SignaturePurchaseController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        
        //Get:
        [HttpGet]
        [Route("signaturepurchases")]
        public IHttpActionResult GetCompanyPurchases(int cid)
        {
           SignaturePurchase SigPurchase = db.SignaturePurchases.Find(cid);
           if(SigPurchase == null)
            {
                return NotFound();
            }

            string Companyinfo = string.Format("{0}{1}{2}{3}{4}", SigPurchase.SignaturePurchaseId, SigPurchase.LicensesPurchased, SigPurchase.PurchaseDate, SigPurchase.PurchaseDate.AddYears(1), SigPurchase.Price);

            return Ok(Companyinfo);
        }

        // GET: api/signaturepurchases
        [HttpGet]
        [Route("signaturepurchases")]
        public IHttpActionResult GetPurchases()
        {
            var result = new List<SignaturePurchaseListShape>();
            foreach (var c in db.SignaturePurchases)
            {
                result.Add(SignaturePurchaseListShape.FromDataModel(c, Request));
            }
            return Ok(result);
        }

        // GET: api/signaturepurchases/5
        [HttpGet]
        [Route("signaturepurchases/{id}")]
        public IHttpActionResult GetSignaturePurchase(int id)
        {
            SignaturePurchase purchase = db.SignaturePurchases.Find(id);
            if (purchase == null)
            {
                return NotFound();
            }
            return Ok(SignaturePurchaseShape.FromDataModel(purchase, Request));
        }

        [HttpPut]
        [Route("signaturepurchases/{id}")]
        public IHttpActionResult UpdateEmployee(int id, SignaturePurchaseShape purchaseShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != purchaseShape.SignaturePurchaseId)
            {
                return BadRequest();
            }
            SignaturePurchase purchase = db.SignaturePurchases.Find(id);
            if (purchase == null)
            {
                return NotFound();
            }

            SignaturePurchaseShape.ToDataModel(purchaseShape, purchase);
            db.SaveChanges();
            return Ok(purchaseShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("signaturepurchases")]
        public IHttpActionResult AddPurchase(SignaturePurchaseShape purchaseShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            SignaturePurchase purchase = SignaturePurchaseShape.ToDataModel(purchaseShape);
            db.SignaturePurchases.Add(purchase);
            db.SaveChanges();
            return Ok(SignaturePurchaseShape.FromDataModel(purchase, Request));
        }

        // DELETE: api/companyusers/5
        [HttpDelete]
        [Route("signaturepurchases/{id}") ]
        public IHttpActionResult DeleteEmployee(int id)
        {
            SignaturePurchase purchase = db.SignaturePurchases.Find(id);
            if (purchase == null)
            {
                return NotFound();
            }
            db.SignaturePurchases.Remove(purchase);
            db.SaveChanges();
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }
    }
}