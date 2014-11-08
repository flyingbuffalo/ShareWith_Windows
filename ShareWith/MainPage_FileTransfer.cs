using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using System.Diagnostics;
using Buffalo.WiFiDirect;


namespace ShareWith
{
    public sealed partial class MainPage : Page 
    {    
        int BLOCK_SIZE = 1024 * 1024;

        internal async Task SendFileToPeerAsync(StreamSocket socket, StorageFile selectedFile)
        {
            ulong sentSize = 0L;

            byte[] buff = new byte[BLOCK_SIZE];
            var prop = await selectedFile.GetBasicPropertiesAsync();

            startProgress();

            using (var writer = new StreamWriter(socket.OutputStream.AsStreamForWrite()))
            {

                writer.AutoFlush = true;
                // 1. Send the filename length
                writer.WriteLine(selectedFile.Name.Length);
                //writer.
                // 2. Send the filename
                writer.WriteLine(selectedFile.Name);
                // 3. Send the file length
                writer.WriteLine(prop.Size);
                // 4. Send the file

                var fileStream = await selectedFile.OpenStreamForReadAsync();

                writer.Flush();
                var dw = new DataWriter(socket.OutputStream);
                await dw.FlushAsync();
                while (fileStream.Position < (long)prop.Size)
                {
                    var rlen = await fileStream.ReadAsync(buff, 0, buff.Length);
                    dw.WriteBytes(buff);
                    await dw.FlushAsync();

                    sentSize += (ulong)rlen;

                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        setProgressValue(sentSize / (double)prop.Size * 100);
                    });
                }

                await dw.StoreAsync();
                await socket.OutputStream.FlushAsync();

                setProgressValue(100);
            }
        }

        internal async Task<string> RecieveFileFromPeerAsync(StreamSocket socket, StorageFolder folder)
        {
            StorageFile file = null;
            //byte[] buff = new byte[BLOCK_SIZE];

            using (var reader = new StreamReader(socket.InputStream.AsStreamForRead()))
            {
                // 1. Read the filename length
                var filenameLength = await reader.ReadLineAsync();

                // 2. Read the filename
                var originalFilename = await reader.ReadLineAsync();

                // 3. Read the file size
                var fileLength = await reader.ReadLineAsync();

                // 4. Read the file
                ulong fileSize = ulong.Parse(fileLength), receivedSize = 0L;
                
                var dr = new DataReader(socket.InputStream);

                startProgress();

                Debug.WriteLine("create file");
                file = await folder.CreateFileAsync(originalFilename, CreationCollisionOption.ReplaceExisting);
                // Download the file

                //using (var fileStream = await file.OpenStreamForWriteAsync()) await file.OpenAsync(FileAccessMode.ReadWrite)
                Debug.WriteLine("open file Stream");
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    Debug.WriteLine("endof open file Stream");

                    while (receivedSize < fileSize)
                    {
                        var lenToRead = Math.Min(BLOCK_SIZE, (float)(ulong.Parse(fileLength) - receivedSize));
                        Debug.WriteLine("load aysnc read stream");
                        await dr.LoadAsync((uint)lenToRead);
                        Debug.WriteLine("load aysnc read stream");
                        var tempBuff = dr.ReadBuffer((uint)lenToRead);
                        Debug.WriteLine("write aysnc file stream");
                        await fileStream.WriteAsync(tempBuff);
                        Debug.WriteLine("1 thread done");
                        receivedSize += (ulong)lenToRead;

                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            setProgressValue(receivedSize / (double)fileSize * 100);
                        });
                        await Task.Delay(100);
                    }
                    fileStream.Dispose();
                }
            }

            return file.Path;
        }

        internal async Task<StorageFile> FileChooser()
        { 

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add("*");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                return file;
            }
            else 
            {
                throw new FileNotFoundException("없지로오옹~ > <");
            }
        }

        internal async Task<StorageFolder> folderChooser()
        {
            // Clear previous returned folder name, if it exists, between iterations of this scenario
            FolderPicker folderPicker = new FolderPicker();

            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                Debug.WriteLine("Picked folder: " + folder.Name);
                return folder;
            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
                throw new FileNotFoundException("없지로오옹~ > <");
            }
        }
    }
}
