using Project.Models;
using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Infrastructure;
using System.Text.RegularExpressions;

namespace Project.Controllers
{
    public class NewsController : Controller
    {
        private const int NewsPerPage = 6;

        public ActionResult Index(int page =1)
        {
            using(var Context = new ProjectDBEntities())
            {
                var DatabaseQuery = Context.News.Where(nx => nx.DeleteDate == null).OrderByDescending(nx => nx.PostedDate);

                var TotalNewsCount = DatabaseQuery.Count();

                if (page == 0)
                    page = 1;

                var NewsIDs = DatabaseQuery.Skip((page - 1) * NewsPerPage).Take(NewsPerPage).Select(t => t.NewsID).ToArray();
                var News = DatabaseQuery.Where(t => NewsIDs.Contains(t.NewsID)).ToList();

                return View(new NewsIndex()
                {
                    News = new PageData<News>(News, TotalNewsCount, page, NewsPerPage)
                });
            }
        }


        
        public ActionResult Show(int id, string slug)
        {
            using (var Context = new ProjectDBEntities())
            {
                var news = Context.News.Where(nx => nx.NewsID == id).FirstOrDefault<News>();
                if (news == null)
                    return HttpNotFound();

                if (!news.NewsSlug.Equals(slug) || news.IsDeleted)
                    return RedirectToAction("Index");

                return View(new NewsShow()
                {
                    News = news
                });
            }
        }
    }
}
