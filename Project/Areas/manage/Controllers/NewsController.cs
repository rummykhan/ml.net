using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Areas.manage.ViewModels;
using Project.Infrastructure;

namespace Project.Areas.manage.Controllers
{
    [Authorize(Roles = "admin")]
    public class NewsController : Controller
    {
        private const int NewsPerPage = 8;
        public ActionResult Index(int page = 1)
        {

            var Context = new ProjectDBEntities();

            var totalNewsCount = Context.News.Count();


            var currentNewsPage = Context.News
                .OrderBy(x => x.PostedDate)
                .Skip((page - 1) * NewsPerPage)
                .Take(NewsPerPage)
                .ToList();

            return View(new NewsIndexViewModel
                       {
                           News = new PageData<News>(currentNewsPage, totalNewsCount, page, NewsPerPage)
                       });

        }

        
        public ActionResult Delete(int newsId)
        {
            using(var Context = new ProjectDBEntities())
            {
                if (!Context.News.Any(n => n.NewsID == newsId))
                    return HttpNotFound();
                else
                {
                    Context.News.Remove(Context.News.Where(nx => nx.NewsID == newsId).FirstOrDefault<News>());
                    Context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create()
        {
            return View("Form", new PostForm() { IsNew = true });
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Form(PostForm model)
        {
            model.IsNew = model.NewsID == null;

            if (!ModelState.IsValid)
                return View(model);


            using (var Context = new ProjectDBEntities())
            {
                var adminGuid = (Context.Administrators.Where(ax => ax.AdminUserName == User.Identity.Name).FirstOrDefault<Administrator>()).AdminID;

                if (model.IsNew)
                {

                    News news = new News()
                    {
                        NewsTitle = model.Title,
                        NewsSlug = model.Slug,
                        NewsContents = model.Contents,
                        PostedBy = adminGuid,
                        PostedDate = DateTime.UtcNow
                    };

                    Context.News.Add(news);
                    
                }
                else
                {
                    var news = Context.News.Where(nx => nx.NewsID == model.NewsID).FirstOrDefault<News>();
                    news.NewsTitle = model.Title;
                    news.NewsSlug = model.Slug;
                    news.NewsContents = model.Contents;
                    news.PostedDate = DateTime.UtcNow;
                    news.PostedBy = adminGuid;
                }
                Context.SaveChanges();
            }
            return RedirectToAction("Index");

        }

        public ActionResult Edit(int id)
        {
            using(var Context = new ProjectDBEntities())
            {
                var news = Context.News.Where(nx => nx.NewsID == id).FirstOrDefault<News>();
                return View("Form", new PostForm()
                    {
                        NewsID = news.NewsID,
                        IsNew = false,
                        Title = news.NewsTitle,
                        Slug = news.NewsSlug,
                        Contents = news.NewsContents
                    });
            }
        }

        public ActionResult Trash(int newsId)
        {
            using(var Context = new ProjectDBEntities())
            {
                var news = Context.News.Where(nx => nx.NewsID == newsId).FirstOrDefault<News>();
                if (news == null)
                    return HttpNotFound();

                news.DeleteDate = DateTime.UtcNow;
                Context.SaveChanges();

                return RedirectToAction("Index");
            }
        }


        public ActionResult Restore(int newsId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var news = Context.News.Where(nx => nx.NewsID == newsId).FirstOrDefault<News>();
                if (news == null)
                    return HttpNotFound();

                news.DeleteDate = null;
                Context.SaveChanges();

                return RedirectToAction("Index");
            }
        }
    }
}