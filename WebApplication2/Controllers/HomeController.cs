using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost("Home/Index")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var uploadSuccess = false;

            using (var stream = file.OpenReadStream())
            {
                uploadSuccess = await UploadToBlob(file.FileName, stream);
            }

            if (uploadSuccess)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        private async Task<bool> UploadToBlob(string filename, Stream stream = null)
        {
            CloudStorageAccount storageAccount = null;
            CloudBlobContainer cloudBlobContainer = null;
            string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container called 'uploadblob' and append a GUID value to it to make the name unique. 
                    cloudBlobContainer = cloudBlobClient.GetContainerReference("uploadblob" + Guid.NewGuid().ToString());
                    await cloudBlobContainer.CreateAsync();

                    // Set the permissions so the blobs are public. 
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(filename);
                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
                catch (StorageException ex)
                {
                    return false;
                }
                finally
                {
                    if (cloudBlobContainer != null)
                    {
                        await cloudBlobContainer.DeleteIfExistsAsync();
                    }
                }
            }
            else
            {
                return false;
            }

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
