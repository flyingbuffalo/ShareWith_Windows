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
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace ShareWith
{
    public sealed partial class MainPage : Page
    {
        MainPage parent;

        int BLOCK_SIZE = 1024;
        internal async Task SendFileToPeerAsync(StreamSocket socket, StorageFile selectedFile)
        {
            byte[] buff = new byte[BLOCK_SIZE];
            var prop = await selectedFile.GetBasicPropertiesAsync();
            using (var dw = new DataWriter(socket.OutputStream))
            {

                // 1. Send the filename length
                dw.WriteInt32(selectedFile.Name.Length); // filename length is fixed
                // 2. Send the filename
                dw.WriteString(selectedFile.Name);
                // 3. Send the file length
                dw.WriteUInt64(prop.Size);
                // 4. Send the file
                var fileStream = await selectedFile.OpenStreamForReadAsync();
                while (fileStream.Position < (long)prop.Size)
                {
                    var rlen = await fileStream.ReadAsync(buff, 0, buff.Length);
                    dw.WriteBytes(buff);
                }
                await dw.FlushAsync();
                await dw.StoreAsync();
                await socket.OutputStream.FlushAsync();
            }
        }

        internal async Task<string> ReceiveFileFromPeer(StreamSocket socket, StorageFolder folder, string outputFilename = null)
        {
            StorageFile file;
            using (var rw = new DataReader(socket.InputStream))
            {
                // 1. Read the filename length
                await rw.LoadAsync(sizeof(Int32));
                var filenameLength = (uint)rw.ReadInt32();
                // 2. Read the filename
                await rw.LoadAsync(filenameLength);
                var originalFilename = rw.ReadString(filenameLength);
                if (outputFilename == null)
                {
                    outputFilename = originalFilename;
                }
                //3. Read the file length
                await rw.LoadAsync(sizeof(UInt64));
                var fileLength = rw.ReadUInt64();
                parent.fileLength = rw.ReadUInt64();
                
                // 4. Reading file
                using (var memStream = await DownloadFile(rw, fileLength))
                {
                    file = await folder.CreateFileAsync(outputFilename, CreationCollisionOption.ReplaceExisting);
                    using (var fileStream1 = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), fileStream1.GetOutputStreamAt(0));
                    }

                    rw.DetachStream();
                }
            }

            return file.Path;
        }

        internal async Task<InMemoryRandomAccessStream> DownloadFile(DataReader rw, ulong fileLength)
        {
            var memStream = new InMemoryRandomAccessStream();

            // Download the file
            while (memStream.Position < fileLength)
            {
                var lenToRead = Math.Min(BLOCK_SIZE, (float)(fileLength - memStream.Position));
                await rw.LoadAsync((uint)lenToRead);
                var tempBuff = rw.ReadBuffer((uint)lenToRead);
                await memStream.WriteAsync(tempBuff);
            }

            return memStream;
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

        internal async Task<StorageFolder> FileSave()
        {
            // Clear previous returned folder name, if it exists, between iterations of this scenario
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add(".docx");
            folderPicker.FileTypeFilter.Add(".xlsx");
            folderPicker.FileTypeFilter.Add(".pptx");
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
