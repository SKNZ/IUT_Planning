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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework.GamerServices;

namespace IUTInfo
{
    /* This class is NOT thread safe */
    public partial class ADE_AMU_IUT_Info_Planning
    {
        public  const uint                       ImageCacheExpiryDelayHours         = 36;
        public  const uint                       IdentifierExpiryDelaySeconds       = 3600;

        public  Action                           NetworkOperationStartedCallback;
        public  Action                           NetworkOperationFinishedCallback;

        public  Dictionary<uint, string>         WeekIdPerMondayDate                { get;         set; }

        public  string                           Identifier                         { get; private set; }
        public  DateTime                         IdentifierAcquireTime              { get; private set; }

        public  Dictionary<uint, BitmapImage>    PlanningImagePerWeekId             = new Dictionary<uint, BitmapImage>();
        public  Dictionary<uint, DateTime>       PlanningAcquireDatePerWeekId       { get;         set; }

        public  uint                             SelectedWeekId                     { get;         set; }

        public Task<BitmapImage> SelectedWeekPlanningImage     
        {
            get
            {
                // Do we have the image and its expiry date saved locally ?
                if (PlanningAcquireDatePerWeekId.ContainsKey(SelectedWeekId) && PlanningImagePerWeekId.ContainsKey(SelectedWeekId))
                {
                    // Has the cache expired ? Do we have Internet access ?
                    if (DateTime.Now.Subtract(PlanningAcquireDatePerWeekId[SelectedWeekId]).Hours > ImageCacheExpiryDelayHours && NetworkInterface.GetIsNetworkAvailable())
                    {
                        // We do. Download it again.
                        return DownloadPlanningImage().ContinueWith(t => PlanningImagePerWeekId[SelectedWeekId]);
                    }

                    // We don't or the cache hasn't expired yet -> return what we have stored.
                    return Task.Run(() => PlanningImagePerWeekId[SelectedWeekId]);
                }

                // We don't have anything locally, let's redownload it if we have Internet.
                if (NetworkInterface.GetIsNetworkAvailable())
                    return DownloadPlanningImage().ContinueWith(t => PlanningImagePerWeekId[SelectedWeekId]);

                // No Internet, nothing can be done.
                throw new NetworkNotAvailableException();
            }
        }

        public uint CurrentWeekId
        {
            get
            {
                var date = DateTime.Now;

                int deltaMonday;
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Sunday: // if we are on sunday or saturday we want next monday's planing
                        deltaMonday = 1;
                        break;
                    case DayOfWeek.Saturday:
                        deltaMonday = 2;
                        break;
                    default:
                        deltaMonday = -(int)date.DayOfWeek + 1;
                        break;
                }

                var targetMonday = date.AddDays(deltaMonday);
                return WeekIdPerMondayDate.Single(pair => pair.Value.Equals(string.Format("{0:D2}/{1:D2}", targetMonday.Day, targetMonday.Month))).Key;
            }
        }

        public void Initialize()
        {
            WeekIdPerMondayDate = new Dictionary<uint, string>();
            if (PlanningAcquireDatePerWeekId == null)
                PlanningAcquireDatePerWeekId = new Dictionary<uint, DateTime>();

            var adeCurrentYear = DateTime.Now.Month > 08 ? DateTime.Now.Year : DateTime.Now.Year - 1;
            var adeCalendarStartDate = new DateTime(adeCurrentYear, 08, 26);

            for (uint i = 0; i < 52; ++i)
            {
                WeekIdPerMondayDate.Add(i, string.Format("{0:D2}/{1:D2}", adeCalendarStartDate.Day, adeCalendarStartDate.Month));
                adeCalendarStartDate = adeCalendarStartDate.AddDays(7);
            }

            if (SelectedWeekId == 0)
                SelectedWeekId = CurrentWeekId;

            if (SelectedEntityID == 0)
                SelectedEntityID = Entities.Single(o => o.Value.Name.Equals("An. 1 - Gr. 4A")).Key;
        }
        
        /**
         * ADE uses an identifier to allow you access to its image data, even though it can be acquired without logging in.
         * It, however, expires after a certain time (one lasted nearly two whole weeks).
         * Such an identifier can be found in the link of the planning image, e.g, for your group.
         * ADE also uses a single cookie, JSESSIONID, to store a session id (duh).
         * 
         **/
        public async Task AcquireIdentifier()
        {
            if (NetworkOperationStartedCallback != null)
                NetworkOperationStartedCallback();

            var cookieContainer = new CookieContainer();
            var clientHandler = new HttpClientHandler {UseCookies = true, CookieContainer = cookieContainer};
            
            var client = new HttpClient(clientHandler);

            // The order must be respected. ADE stores your "progress" on serverside or something like that.
            var pagesToWalkTrough = new[]
            {
                // Etablish first connection to AMU's ADE planning tool, this allows us to acquire the JSESSIONID cookie for an anonymous connection.
                "http://planning.univ-amu.fr/ade/custom/modules/plannings/anonymous_cal.jsp?resources=1467&projectId=26&startDay=26&startMonth=08&startYear=2013&endDay=25&endMonth=08&endYear=2014&calType=ical",

                // Select the Items (category5) category...
                "http://planning.univ-amu.fr/ade/standard/gui/tree.jsp?category=category5&expand=false&forceLoad=false&reload=false&scroll=0",

                // ... select the "IUT Info"...
                "http://planning.univ-amu.fr/ade/standard/gui/tree.jsp?branchId=2012&expand=false&forceLoad=false&reload=false&scroll=0",

                // ... select "MS" (dunno what it is, but this is actually the shortest path towards selection) ...
                "http://planning.univ-amu.fr/ade/standard/gui/tree.jsp?selectId=2031&reset=false&forceLoad=true&scroll=0",

                // ... select days (no parameters => monday to friday)
                "http://planning.univ-amu.fr/ade/custom/modules/plannings/pianoDays.jsp",
            };

            foreach (var page in pagesToWalkTrough)
            {
                var response = await client.GetAsync(page);
                response.EnsureSuccessStatusCode();
            }

            var imagePageResponse = await client.GetAsync("http://planning.univ-amu.fr/ade/custom/modules/plannings/imagemap.jsp");
            imagePageResponse.EnsureSuccessStatusCode();

            var imagePage = await imagePageResponse.Content.ReadAsStringAsync();

            /**
             * This was originally done through XML parsing of the document.
             * However, as ADE can't be bothered to respect xHTML standards, well, fuck it.
             * 
             * @todo Use a regex.
             * 
             **/
            var identifier = imagePage.Substring(imagePage.IndexOf("/ade/imageEt?identifier=") + "/ade/imageEt?identifier=".Length);

            Identifier = identifier.Substring(0, identifier.IndexOf('&'));
            IdentifierAcquireTime = DateTime.Now;

            if (NetworkOperationFinishedCallback != null)
                NetworkOperationFinishedCallback();
        }


        private async Task DownloadPlanningImage()
        {
            if (NetworkOperationStartedCallback != null)
                NetworkOperationStartedCallback();

            PlanningImagePerWeekId[SelectedWeekId] = await DownloadPlanningImage(SelectedWeekId);
            PlanningAcquireDatePerWeekId[SelectedWeekId] = DateTime.Now;

            Save("Planning.xml");

            if (NetworkOperationFinishedCallback != null)
                NetworkOperationFinishedCallback();
        }

        private async Task<BitmapImage> DownloadPlanningImage(uint weekId)
        {
            if (string.IsNullOrEmpty(Identifier) || DateTime.Now.Subtract(IdentifierAcquireTime).Seconds > 3600)
                await AcquireIdentifier();
            
            /**
             * There is a load of space (~35%) at the bottom of the image sent by the ADE Campus software that is used for planning from 20h30 to 00h00.
             * As there are no classes at times, these area might be removed.
             * To use all the screen space, we add 35% to the requested image size and remove them locally.
             * The end result is a planning image that use the whole screen height and doesn't show useless late-night hours garbage.
             * 
             **/
            const double cropAreaHeightPercentage = 0.34543670265d;
            
            var imageWidth = (int) Math.Truncate(Application.Current.RootVisual.RenderSize.Width);
            var imageHeight = (int)Math.Truncate(Application.Current.RootVisual.RenderSize.Height * (1 + cropAreaHeightPercentage));

            var imageURI = string.Format("http://planning.univ-amu.fr/ade/imageEt?identifier={0}&projectId=26&idPianoWeek={1}&idPianoDay=0,1,2,3,4,5&idTree={2}&width={3}&height={4}&lunchName=PHALLUS&displayMode=1057855&showLoad=false&ttl=1379893965988000&displayConfId=8",
                                                                                            Identifier,             weekId,                      SelectedEntityID, imageWidth, imageHeight);

            var streamRequest = await new HttpClient().GetAsync(imageURI);
            streamRequest.EnsureSuccessStatusCode();

            var stream = await streamRequest.Content.ReadAsStreamAsync();
            
            var imageBuffer = new BitmapImage();
            imageBuffer.SetSource(stream);

            var image = new WriteableBitmap(imageBuffer);
            image = image.Crop(0, 0, image.PixelWidth, (int)(image.PixelHeight * (1 - cropAreaHeightPercentage)));

            using (var memoryStream = new MemoryStream())
            {
                image.SaveJpeg(memoryStream, image.PixelWidth, image.PixelHeight, 0, 100);

                imageBuffer = new BitmapImage();
                imageBuffer.SetSource(memoryStream);

                return imageBuffer;
            }
        }

        public bool MoveNextWeek()
        {
            if (!NetworkInterface.GetIsNetworkAvailable() && !PlanningImagePerWeekId.ContainsKey(SelectedWeekId + 1))
                throw new NetworkNotAvailableException();

            if (SelectedWeekId == 51)
                return false;

            ++SelectedWeekId;

            return true;
        }

        public bool MovePreviousWeek()
        {
            if (!NetworkInterface.GetIsNetworkAvailable() && !PlanningImagePerWeekId.ContainsKey(SelectedWeekId - 1))
                throw new NetworkNotAvailableException();

            if (SelectedWeekId == 0)
                return false;

            --SelectedWeekId;

            return true;
        }
    }
}
