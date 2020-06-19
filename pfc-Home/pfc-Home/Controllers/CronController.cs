using pfc_Home.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace pfc_Home.Controllers
{
    public class CronController : Controller
    {
        // GET: Cron
        // https://pfc00sa.hopto.org/cron/RunEveryMin runs every 2mins not 1 mins since free account allows a min of 2mins
        public ActionResult RunEveryMin()
        {
            PubSubRepository psr = new PubSubRepository();
            psr.DownloadEmailFromQueueAndSend();

            return Content("Sent");
        }
    }
}