using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;

public class ADLSOperations
{
    private static DataLakeServiceClient CreatesDataLakeConnection()
    {
        //Retrieving environment variables
        var storageAccountName = System.Environment.GetEnvironmentVariable("storageAccountName");
        var storageAccountKey = System.Environment.GetEnvironmentVariable("storageAccountKey");
        var storageServiceUri = System.Environment.GetEnvironmentVariable("storageServiceUri");

        //Creates a shared key credential to access the storage account on behalf of the application
        StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);

        //Create DataLakeServiceClient using StorageSharedKeyCredentials
        DataLakeServiceClient serviceClient = new DataLakeServiceClient(new Uri(storageServiceUri), sharedKeyCredential);

        return serviceClient;
    }

    private static DataLakeDirectoryClient GetsReferenceToContainer(DataLakeServiceClient serviceClient, string storageRootContainer)
    {
        DataLakeDirectoryClient directoryClient = serviceClient.GetFileSystemClient(storageRootContainer).GetDirectoryClient("");

        return directoryClient;
    }

    private static DataLakeFileSystemClient GetsReferenceToFileSystem(DataLakeServiceClient serviceClient, string storageRootContainer)
    {
        DataLakeFileSystemClient fileSystem = serviceClient.GetFileSystemClient(storageRootContainer);

        return fileSystem;
    }

    public static bool AddsFolderOwnerToContainerACLAsExecute(string folderOwner, string container, bool isDefaultScope, string storageRootContainer)
    {
        var serviceClient = CreatesDataLakeConnection();
        
        var directoryClient = GetsReferenceToContainer(serviceClient, storageRootContainer);

        List<PathAccessControlItem> accessControlListUpdate = new List<PathAccessControlItem>()
        {
            new PathAccessControlItem(AccessControlType.User, RolePermissions.Execute, isDefaultScope,entityId: folderOwner)
        };

        //Update root container's ACL
        var result = directoryClient.UpdateAccessControlRecursive(accessControlListUpdate, null);
 
        if(result.GetRawResponse().Status != 200)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool CreatesNewFolder(string folder, string storageRootContainer)
    {
        var serviceClient = CreatesDataLakeConnection();

        var directoryFileSystem = GetsReferenceToFileSystem(serviceClient, storageRootContainer);

        DataLakeDirectoryClient directory = directoryFileSystem.CreateDirectory(folder);
        
        var response = directory.Create();

        if(response.GetRawResponse().Status != 200 || response.GetRawResponse().Status != 201)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool AssignsRWXToFolderOwner(string folderOwner, string folder)
    {
        return true;
    }

    public static bool SavesFundCodeIntoContainerMetadata(string fundCode, string container)
    {
        return true;
    }

    public static async Task ManageDirectoryACLs(DataLakeFileSystemClient fileSystemClient)
    {
        var directoryClient = fileSystemClient.GetDirectoryClient("");
        var directoryAccessControl = await directoryClient.GetAccessControlAsync();
        foreach (var item in directoryAccessControl.Value.AccessControlList)
        {
            Console.WriteLine(item.ToString());
        }

        var accessControlList = PathAccessControlExtensions.ParseAccessControlList("user::rwx,group::r-x,other::rw-");

        directoryClient.SetAccessControlList(accessControlList);

    }

    private static string ReturnStorageAccountName()
    {
        return System.Environment.GetEnvironmentVariable("storageAccountName", EnvironmentVariableTarget.Process);
    }

    private static string ReturnStorageAccountKey()
    {
        return System.Environment.GetEnvironmentVariable("storageAccountKey", EnvironmentVariableTarget.Process);
    }

    private static string ReturnStorageAccountUri()
    {
        return System.Environment.GetEnvironmentVariable("serviceUri", EnvironmentVariableTarget.Process);
    }

    private static string ReturnStorageAccountContainer()
    {
        return System.Environment.GetEnvironmentVariable("container", EnvironmentVariableTarget.Process);
    }
}