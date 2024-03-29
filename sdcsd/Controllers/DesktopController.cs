﻿/***********************************************************************
 * Copyright (C) 2011 Ari Rokosa, Chinedu Eze, Johannes Virtanen,
 *                    Petri Tuononen, Timo Korkalainen
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using sdcsd.Models;

namespace sdcsd.Controllers
{

    public class DesktopController : Controller
    {
        //
        // GET: /Desktop/

        private DesktopItemModelDBContext _db = new DesktopItemModelDBContext();
        private DesktopDBContext _dbDesktop = new DesktopDBContext();

        public ActionResult Index()
        {
            ViewBag.login = false;
            if (Session["loggedin"] == "true")
                ViewBag.login = true;

            ViewBag.ActiveDesktop = GetActiveDesktop();

            var items = _db.DesktopItems.ToList<DesktopItemModel>();
            return View(items);
        }

        public FilePathResult GetFileFromDisk(string id)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Content/desktopFiles/";
            string fileName = id;
            return File(path + fileName, "text/plain", id);
        }
   
        public ActionResult AddItem()
        {
            if(Session["loggedin"] == "true")
                return View();

            return null;
        }

        [HttpPost]
        public string UpdateLocation(string name, string locX, string locY)
        {
            if (_db.DesktopItems.Count(i => i.Name == name) > 0)
            {
                DesktopItemModel item = _db.DesktopItems.First(i => i.Name == name);
                item.LocX = locX;
                item.LocY = locY;
                _db.SaveChanges();
            }

            return "";
        }

        [HttpPost]
        public string DeleteItem(string name)
        {
            if (_db.DesktopItems.Count(i => i.Name == name) > 0)
            {
                DesktopItemModel item = _db.DesktopItems.First(i => i.Name == name);
                _db.DesktopItems.Remove(item);
                _db.SaveChanges();
            }

            var path = Server.MapPath("~/Content/desktopFiles/"+name);
            try
            {
                System.IO.File.Delete(path);
            }
            catch
            {
            }

            return "";
        }

        [HttpPost]
        public ActionResult Upload(string qqfile)
        {
            var path = Server.MapPath("~/Content/desktopFiles");
            var file = string.Empty;

            try
            {
                var stream = Request.InputStream;
                if (String.IsNullOrEmpty(Request["qqfile"]))
                {
                    //IE
                    HttpPostedFileBase postedFile = Request.Files[0];
                    stream = postedFile.InputStream;
                    file = Path.Combine(path, System.IO.Path.GetFileName(Request.Files[0].FileName));
                }
                else
                {
                    //Webkit, Mozilla
                    file = Path.Combine(path, qqfile);
                }
                
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                System.IO.File.WriteAllBytes(file, buffer);

                DesktopItemModel newItem = new DesktopItemModel();
                newItem.DesktopID = 1; //reflect to desktop which was active when item added
                newItem.Name = qqfile;
                newItem.LocX = "45%";
                newItem.LocY = "45%";

                _db.DesktopItems.Add(newItem);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, "application/json");
            }

            return Json(new { success = true }, "text/html");
        }

        public int GetActiveDesktop()
        {
            try
            {
                if (Session["loggedin"] == "false")
                    return 1; //return default

                string user = Session["user"].ToString();
                if (_dbDesktop.Desktops.Count(i => i.UserName == user) > 0)
                {
                    DesktopModel item = _dbDesktop.Desktops.First(i => i.UserName == user);
                    return item.DesktopID;
                }
                return 1;
            }
            catch
            {
                return 1;
            }
        }

    }
}
