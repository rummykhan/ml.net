using Project.Areas.manage.ViewModels;
using Project.Infrastructure;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Areas.manage.Controllers
{
    [Authorize(Roles="admin")]
    public class GenereController : Controller
    {
        private const int GenerePerPage = 8;
        public ActionResult Index(int page=1)
        {
            var Context = new ProjectDBEntities();

            var totalGeneresCount = Context.Generes.Count();


            var currentNewsPage = Context.Generes
                .OrderBy(x => x.GenereID)
                .Skip((page - 1) * GenerePerPage)
                .Take(GenerePerPage)
                .ToList();

            return View(new GenereIndex
            {
                Genere = new PageData<Genere>(currentNewsPage, totalGeneresCount, page, GenerePerPage)
            });
        }

        public ActionResult Delete(int genereId)
        {
            using (var Context = new ProjectDBEntities())
            {
                if (!Context.Generes.Any(n => n.GenereID == genereId))
                    return HttpNotFound();
                else
                {
                    Context.Generes.Remove(Context.Generes.Where(nx => nx.GenereID == genereId).FirstOrDefault<Genere>());
                    Context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create()
        {
            return View("Form", new GenerePostForm() { IsNew = true });
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Form(GenerePostForm model)
        {
            model.IsNew = model.GenereID == null;

            using (var Context = new ProjectDBEntities())
            {
                var validation = Context.Generes.Where(gx => gx.GenereDetail == model.Detail).FirstOrDefault<Genere>();
                if (validation != null)
                    ModelState.AddModelError("Detail", "This \"Genere\" already exist.. ");

                if (!ModelState.IsValid)
                    return View(model);

                if (model.IsNew)
                {
                    Genere genere = new Genere()
                    {
                        GenereDetail = model.Detail,
                        IsActive = model.IsActive
                    };

                    Context.Generes.Add(genere);
                }
                else
                {
                    var genere = Context.Generes.Where(nx => nx.GenereID == model.GenereID).FirstOrDefault<Genere>();
                    genere.GenereDetail = model.Detail;
                    genere.IsActive = model.IsActive;
                }

                Context.SaveChanges();
            }
            return RedirectToAction("Index");

        }

        public ActionResult Edit(int id)
        {
            using (var Context = new ProjectDBEntities())
            {
                var genere = Context.Generes.Where(nx => nx.GenereID == id).FirstOrDefault<Genere>();
                return View("Form", new GenerePostForm()
                {
                    IsNew = false,
                    GenereID = genere.GenereID,
                    Detail = genere.GenereDetail,
                    IsActive = genere.IsActive
                });
            }
        }

        public ActionResult Trash(int genereId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var news = Context.Generes.Where(nx => nx.GenereID == genereId).FirstOrDefault<Genere>();
                if (news == null)
                    return HttpNotFound();

                news.IsActive = false;
                Context.SaveChanges();

                return RedirectToAction("Index");
            }
        }


        public ActionResult Restore(int genereId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var news = Context.Generes.Where(nx => nx.GenereID == genereId).FirstOrDefault<Genere>();
                if (news == null)
                    return HttpNotFound();

                news.IsActive = true;
                Context.SaveChanges();

                return RedirectToAction("Index");
            }
        }

	}
}