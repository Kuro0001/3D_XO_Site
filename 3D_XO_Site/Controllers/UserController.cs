using _3D_XO_Site.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace _3D_XO_Site.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult BanPage()
        {
            return View();
        }
        // GET: User
        public ActionResult Index()
        {
            return View(LoadFromAPI());
        }
        // GET: User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        public ActionResult Create(User note)
        {
            if (note.UserName != null && note.Password != null && note.Email != null)
            {
                note.IsActive = true;
                note.Violation = 0;
                HttpStatusCode code = SendToAPI(note);
                switch (code)
                {
                    case HttpStatusCode.NoContent:
                        {
                            return RedirectToAction("Index");
                        }
                    case HttpStatusCode.OK:
                        {
                            return RedirectToAction("Index");
                        }
                    default:
                        {
                            return BadRequest();
                        }
                }
            }
            return View();
        }

        // GET: User/Details/5
        [HttpGet]
        public ActionResult Details(string usarName)
        {
            if (usarName == null)
            {
                return BadRequest();
            }
            else
            {
                User block = LoadFromAPI(usarName);
                if (block == null)
                    return NotFound();
                else
                    return View(block);
            }
        }
        [HttpPost]
        public ActionResult Details(User block)
        {
            if (block.UserName != null)
            {
                HttpStatusCode code = SendToAPIToAppdate(block);
                switch (code)
                {
                    case HttpStatusCode.NoContent:
                        {
                            return RedirectToAction("Index");
                        }
                    case HttpStatusCode.OK:
                        {
                            return RedirectToAction("Index");
                        }
                    default:
                        {
                            return BadRequest();
                        }
                }
            }
            return View(block);
        }







        // GET: User/LogIn
        [HttpGet]
        public ActionResult LogIn()
        {
            if (Settings.token != null)
                Settings.token = null;
            return View();
        }
        // POST: User/LogIn
        [HttpPost]
        public ActionResult LogIn(User note)
        {
            if (note.UserName != null && note.Password != null)
            {
                string token = LoginToWebApi(note.UserName, note.Password);
                if (token != null)
                {
                    if (token == "notAllowed")
                    {
                        return Redirect("/User/BanPage");
                    }
                    Settings.token = token;
                    return RedirectPermanent("/Home");
                }
            }
            return BadRequest();
        }
        public string LoginToWebApi(string username, string password)
        {
            string url = String.Format("{0}/api/authenticate/login", Settings.url);
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/xml";
            string data = $"<LoginModel><Username>{username}</Username><Password>{password}</Password></LoginModel>";
            using (StreamWriter streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }
            string result = null;
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    {
                        return "notAllowed";
                    }
            }
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                JsonDocument doc = JsonDocument.Parse(result);
                JsonElement root = doc.RootElement;
                return root.GetProperty("token").ToString();
            }
            return null;
        }
        public List<User> LoadFromAPI()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://localhost:44391/api/authenticate/get-list-gamers-violation");
            request.Accept = "application/xml";
            request.Method = "GET";
            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);
            WebResponse response = request.GetResponse();
            XmlDocument doc = new XmlDocument();
            doc.Load(response.GetResponseStream());
            XmlElement root = doc.DocumentElement;
            List<User> listU = new List<User>();
            
            foreach (XmlElement node in root)
            {
                User block = new User()
                {
                    Id = node["Id"].InnerXml,
                    UserName = node["UserName"].InnerXml,
                    Email = node["Email"].InnerXml,
                    IsActive = Convert.ToBoolean(node["IsActive"].InnerXml),
                    Violation = Convert.ToInt32(node["Violation"].InnerXml)
                };
                listU.Add(block);
            }
            return listU;
        }
        public User LoadFromAPI(string userName)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}/api/authenticate/get-user-by-username/{1}", Settings.url, userName));
            request.Accept = "application/xml";
            request.Method = "GET";
            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);
            WebResponse response = request.GetResponse();
            XmlDocument doc = new XmlDocument();
            doc.Load(response.GetResponseStream());
            XmlElement root = doc.DocumentElement;
            User block = new User()
            {
                Id = root["Id"].InnerXml,
                UserName = root["UserName"].InnerXml,
                Email = root["Email"].InnerXml,
                IsActive = Convert.ToBoolean(root["IsActive"].InnerXml),
                Violation = Convert.ToInt32(root["Violation"].InnerXml)
            };
            return block;
        }


        private HttpStatusCode SendToAPI(User note)
        {
            //формируем xml документ
            XmlDocument doc = new XmlDocument();
            doc.CreateXmlDeclaration("1.0", "utf-8", "no");
            XmlElement workNode = PackToXml(doc, note);
            doc.AppendChild(workNode);
            //получаем строку в формате XML
            string xml = doc.OuterXml;
            //преобразуем строку в массив байт
            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            //создаем http запрос, указываем метод и тип содержимого
            string url = String.Format("{0}/api/authenticate/register-admin/", Settings.url);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);

            request.Method = "POST";
            request.ContentType = "application/xml; encoding=utf-8";
            request.ContentLength = byteArray.Length;
            //в поток (в тело) запроса записываем подготовленный массив байт
            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);
            reqStream.Close();
            //отправляем запрос и получаем ответ
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.StatusCode;
        }


        private XmlElement PackToXml(XmlDocument doc, User note)
        {
            XmlElement workNode = doc.CreateElement("RegisterModel");

            XmlElement Node1 = doc.CreateElement("Username");
            Node1.InnerText = note.UserName;
            workNode.AppendChild(Node1);

            XmlElement Node2 = doc.CreateElement("Email");
            Node2.InnerText = note.Email;
            workNode.AppendChild(Node2);

            XmlElement Node3 = doc.CreateElement("Password");
            Node3.InnerText = note.Password;
            workNode.AppendChild(Node3);

            return workNode;
        }
        private HttpStatusCode SendToAPIToAppdate(User note)
        {
            string url = String.Format("{0}/api/authenticate/set-user/{1}&{2}", Settings.url, note.UserName, note.IsActive);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            UserController user = new UserController();
            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);
            else
                return HttpStatusCode.BadRequest;
            request.Method = "PUT";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.StatusCode;
        }
        private XmlElement PackToXmlUpdate(XmlDocument doc, User note)
        {
            XmlElement workNode = doc.CreateElement("RegisterModel");

            XmlElement Node1 = doc.CreateElement("Username");
            Node1.InnerText = note.UserName;
            workNode.AppendChild(Node1);

            XmlElement Node2 = doc.CreateElement("Email");
            Node2.InnerText = note.Email;
            workNode.AppendChild(Node2);

            XmlElement Node3 = doc.CreateElement("Password");
            Node3.InnerText = note.Password;
            workNode.AppendChild(Node3);

            return workNode;
        }
    }
}
