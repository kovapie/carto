using System.Linq;
using System.Web.Mvc;
using QuickGraph.Graphviz;
using carto.Models;

namespace carto.Controllers
{
    public class InventoryController : Controller
    {
        //
        // GET: /Inventory/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }

        //
        // GET: /Inventory/Details/5

        public ActionResult Details(long id = 0)
        {
            ViewBag.repository = CmdbRepository.Instance;
            var item = CmdbRepository.Instance.Graph.Vertices.FirstOrDefault(c => c.Id == id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        public ActionResult PartialDetails(long id = 0)
        {
            var item = CmdbRepository.Instance.Graph.Vertices.FirstOrDefault(c => c.Id == id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return PartialView(item);
        }

        //
        // GET: /Inventory/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Inventory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CmdbItem item)
        {
            if (ModelState.IsValid)
            {
                CmdbRepository.Instance.Graph.AddVertex(item);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(item);
        }

        //
        // GET: /Inventory/Edit/5

        public ActionResult Edit(long id = 0)
        {
            var item = CmdbRepository.Instance.Graph.Vertices.FirstOrDefault(c => c.Id == id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        //
        // POST: /Inventory/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CmdbItem item)
        {
            if (ModelState.IsValid)
            {
                var current = CmdbRepository.Instance.Graph.Vertices.FirstOrDefault(c => c.Id == item.Id);
                //CmdbRepository.Instance.Graph.Replace(item);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        //
        // GET: /Inventory/Delete/5

        public ActionResult Delete(long id = 0)
        {
            ViewBag.repository = CmdbRepository.Instance;
            var item = CmdbRepository.Instance.Graph.Vertices.FirstOrDefault(c => c.Id == id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        //
        // POST: /Inventory/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            var applicationcomponent = CmdbRepository.Instance.Graph.Vertices.FirstOrDefault(c => c.Id == id);
            //CmdbRepository.Instance.Graph.RemoveVertex(item);
            //db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}