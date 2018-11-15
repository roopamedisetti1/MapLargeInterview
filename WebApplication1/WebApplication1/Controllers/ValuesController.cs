using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Threading.Tasks;

namespace Interview2.Controllers
{
    public class ValuesController : ApiController
    {


        // GET api/values
        public IEnumerable<String> Get()
        {
            //I am doing default directory
            DriveInfo driveInfo = new DriveInfo(@"C:\");

            DirectoryInfo rootDirect = driveInfo.RootDirectory;

            DirectoryInfo[] listOfDirectories = rootDirect.GetDirectories("*");

             List<string> files = new List<string>();

             foreach (DirectoryInfo d in listOfDirectories)
             {
               files.Add(d.Name);
             
             }

           
             return files.ToArray();           


        }

        // GET api/values/5
        public IEnumerable<String> GetFolders(string directoryName )
        {
            DirectoryInfo dirInfo = new DirectoryInfo("C:\\"+ directoryName);           

            List<string> fileList = new List<string>();

            SearchDirectory(dirInfo, fileList);

            if (fileList.Count > 0)
            {
                return fileList.ToArray();
            }
            else
                return null;
        }


        private void SearchDirectory(DirectoryInfo dirInfo, List<string> fileList)
        {
            try
            {
                foreach (DirectoryInfo subdirInfo in dirInfo.GetDirectories())
                {
                    SearchDirectory(subdirInfo, fileList);
                }
            }
            catch
            {
                Console.WriteLine("Access Denied ");
            }
            try
            {
                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                {
                    fileList.Add(fileInfo.FullName + "=>  File Size : "+ fileInfo.Length);
                    
                }
            }
            catch
            {
                Console.WriteLine("Access Denied ");
            }
        }



        // GET api/Download/filePath
        [HttpPost]
        [ActionName("Downloadfile")]
        public Task<HttpResponseMessage> Downloadfile(string filePath)
        {

            HttpResponseMessage result = null;

            try
            {

                // check if parameter is valid
                if (String.IsNullOrEmpty(filePath))
                {
                 result = Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                else if (!File.Exists(filePath))
                {
                    result = Request.CreateResponse(HttpStatusCode.Gone);
                }
                else
                {
                    // Serve the file to the client
                      result = Request.CreateResponse(HttpStatusCode.OK);
                      result.Content = new StreamContent(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                     result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                     result.Content.Headers.ContentDisposition.FileName = "MarLargeInterview";
                     result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                }

                 return Task.FromResult(result);

            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        // POST api/values
        [HttpPost]
        [ActionName("UploadFile")]
        public Task<HttpResponseMessage> UploadFile()
        {
            List<string> savefilepath = new List<string>();

            if (!Request.Content.IsMimeMultipartContent())

            {

                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            }

            string rootpath = HttpContext.Current.Server.MapPath("~/FilesUploaded");

            var provider = new MultipartFileStreamProvider(rootpath);

            var task = Request.Content.ReadAsMultipartAsync(provider).

                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }


                    foreach (MultipartFileData item in provider.FileData)
                    {
                        try
                        {

                            string name = item.Headers.ContentDisposition.FileName.Replace("\"", "");

                            string newfilename = Guid.NewGuid() + Path.GetExtension(name);

                            File.Move(item.LocalFileName, Path.Combine(rootpath, newfilename));

                            Uri baseuri = new Uri(Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, string.Empty));

                            string fileRelativePath = "~/FilesUploaded/" + newfilename;

                            Uri filefullpath = new Uri(baseuri, VirtualPathUtility.ToAbsolute(fileRelativePath));

                            savefilepath.Add(filefullpath.ToString());

                        }
                        catch (Exception)
                        {
                            throw;
                        }

                    }

                    return Request.CreateResponse(HttpStatusCode.Created, savefilepath);

                });

            return task;
        }

            // PUT api/values/5
            public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
