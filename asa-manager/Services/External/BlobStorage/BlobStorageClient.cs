// -----------------------------------------------------------------------
// <copyright file="BlobStorageClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage
{
    public class BlobStorageClient : IBlobStorageClient
    {
        private readonly BlobServiceClient client;

        public BlobStorageClient(AppConfig config)
        {
            this.client = new BlobServiceClient(config.Global.StorageAccountConnectionString);
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                var accountInfo = await this.client.GetAccountInfoAsync();
                if (accountInfo != null)
                {
                    return new StatusResultServiceModel(true, "Alive and well!");
                }
                else
                {
                    return new StatusResultServiceModel(false, $"Unable to retrieve Storage Account Information from the Blob Storage Client Container.");
                }
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, $"Table Storage check failed: {e.Message}");
            }
        }

        public async Task CreateBlobAsync(string blobContainerName, string contentFileName, string blobFileName)
        {
            BlobContainerClient createContainerClient = this.client.GetBlobContainerClient(blobContainerName);
            await createContainerClient.CreateIfNotExistsAsync();

            // Get the client for writing to this container
            BlobClient createBlobClient = createContainerClient.GetBlobClient(blobFileName);

            try
            {
                FileStream uploadStream = File.OpenRead(contentFileName);
                await createBlobClient.UploadAsync(uploadStream);
                uploadStream.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to upload the contents of temporary local file {tempFileName} to blob storage.", e);
            }
        }
    }
}