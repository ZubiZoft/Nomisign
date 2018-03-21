using CfdiService.Filters;
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
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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
        [Route("signaturepurchases/{cid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetSignaturePurchase(int cid)
        {
            List<SignaturePurchase> purchase = db.SignaturePurchases.Where(x => x.CompanyId == cid).ToList();

            var pShape = new List<SignaturePurchaseShape>();

            foreach (var p in purchase)
            {
                pShape.Add(SignaturePurchaseShape.FromDataModel(p, Request));
            }

            return Ok(pShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("signaturepurchases")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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