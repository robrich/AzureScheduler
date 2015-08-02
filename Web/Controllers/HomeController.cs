namespace AzureScheduler.Web.Controllers {
	using System.Configuration;
	using System.Web.Mvc;
	using System.Web.Security;

	public class HomeController : Controller {

		public ActionResult Index() {
			return this.View();
		}

		[HttpPost]
		public ActionResult Index(string Username, string Password) {
			// This auth is quite lame, but this site doesn't need more
			if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && Username == ConfigurationManager.AppSettings["LoginUsername"] && Password == ConfigurationManager.AppSettings["LoginPassword"]) {
				FormsAuthentication.SetAuthCookie(Username, createPersistentCookie: false);
				return RedirectToAction("Index", "Azure");
			} else {
				return RedirectToAction("Index", "Home");
			}
		}

		public ActionResult Logout() {
			FormsAuthentication.SignOut();
			return RedirectToAction("Index", "Home");
		}

	}
}
