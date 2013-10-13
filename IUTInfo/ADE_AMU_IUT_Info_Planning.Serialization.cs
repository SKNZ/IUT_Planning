/*	
	This file is part of IUTInfo.

	IUTInfo is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	IUTInfo is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with IUTInfo.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Media.Imaging;
using Polenter.Serialization;
using System.Net.NetworkInformation;

namespace IUTInfo
{
    partial class ADE_AMU_IUT_Info_Planning
    {
        public static ADE_AMU_IUT_Info_Planning Load(string fileName)
        {
            using (var fileStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!fileStorage.FileExists(fileName))
                    return null;

                try
                {
                    using (var stream = fileStorage.OpenFile(fileName, FileMode.Open))
                    {
                        var serializer = new SharpSerializer();
                        return serializer.Deserialize(stream) as ADE_AMU_IUT_Info_Planning;
                    }
                }
                catch (Exception ex)
                {
                    //if (Debugger.IsAttached)
                    //    Debugger.Break();

                    fileStorage.DeleteFile(fileName);
                }
            }

            return null;
        }

        public bool ResetCache()
        {
            using (var fileStorage = IsolatedStorageFile.GetUserStoreForApplication())
                foreach (var file in fileStorage.GetFileNames("planning-*"))
                    fileStorage.DeleteFile(file);

            PlanningAcquireDatePerWeekId.Clear();
            PlanningImagePerWeekId.Clear();

            return NetworkInterface.GetIsNetworkAvailable();
        }

        public void Save(string fileName)
        {
            using (var fileStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (fileStorage.FileExists(fileName))
                    fileStorage.DeleteFile(fileName);

                using (var stream = fileStorage.OpenFile(fileName, FileMode.Create))
                {
                    var serializer = new SharpSerializer();
                    serializer.Serialize(this, stream);
                }
            }
        }
        
        /**
         * @warning Most likely slow as hell, for serialization purposes only.
         * Should be done manually, but I'm just lazy like that.
         * 
         */
        public Dictionary<uint, String> PlanningImagePerWeekIdSerializationDecoy
        {
            get
            {
                return PlanningImagePerWeekId.ToDictionary(pair => pair.Key, pair =>
                {
                    using (var fileStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        var fileName = string.Format("planning-{0}-{1}.jpg", SelectedEntityID, pair.Key);

                        if (fileStorage.FileExists(fileName))
                            fileStorage.DeleteFile(fileName);

                        using (var stream = fileStorage.OpenFile(fileName, FileMode.Create))
                            new WriteableBitmap(pair.Value).SaveJpeg(stream, pair.Value.PixelWidth, pair.Value.PixelHeight, 0, 100);

                        return fileName;
                    }
                });
            }
            set
            {
                foreach (var entry in value)
                {
                    using (var fileStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!fileStorage.FileExists(entry.Value))
                            continue;

                        using (var fileStream = fileStorage.OpenFile(entry.Value, FileMode.Open))
                        {
                            var bmp = new BitmapImage();
                            bmp.SetSource(fileStream);
                            PlanningImagePerWeekId.Add(entry.Key, bmp);
                        }
                    }
                }
            }
        }
    }
}
