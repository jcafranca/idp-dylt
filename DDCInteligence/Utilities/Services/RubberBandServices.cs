using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using DDCInteligence.Utilities.Adorners;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.View;
using DDCInteligence.ViewModel;
using DotNetCS;
using DotNetCS.ImageViewer;
using HandyControl.Tools;
using ICSharpCode.AvalonEdit.Editing;
using Mysqlx.Session;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
namespace DDCInteligence.Utilities.Services
{
    public class RubberBandServices : RubberBandModel, IRubberBand
    {
        private object focusedParent;
        private object focusedSource;
        private RubberBandAdorner focusedAdorner;

        private object targetSource;
        private object targetDataContext;
        private string targetPropertyName;

        public enum DataContextType
        {
            Unknown,
            Consignee,
            Shipper,
            BillTo,
            Instruction,
            Items,
            Reference
        }

        // Cache for reflection performance
        private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyCache = new ConcurrentDictionary<string, PropertyInfo>();

        public void FocusTargetElement()
        {
            (targetSource as UIElement)?.Focus();
        }
        public void SetFocus(object parent, object source)
        {
            if (source is TextBox == false)
            {
                return;
            }

            if (focusedSource != null && focusedAdorner != null)
            {
                AdornerLayer.GetAdornerLayer((FrameworkElement)focusedSource)?.Remove(focusedAdorner);
            }

            var a = (FrameworkElement)source;
            var adornerLayer = AdornerLayer.GetAdornerLayer(a);
            var adorner = new RubberBandAdorner(a);
            var b = BindingOperations.GetBindingExpression(a, TextBox.TextProperty);
            var p = b.ParentBinding.Path.Path;
            adornerLayer.Add(adorner);

            focusedParent = parent;
            focusedSource = source;
            focusedAdorner = adorner;
        }
        public void GetRubberBand(RubberBandFetched message)
        {
            try
            {
                if (ViewModelLocator.Instance.Process.CurrentState != EState.IDLE)
                {
                    var pcbox = ViewModelLocator.Instance.ImageViewer.pictureBox;
                    var words = pcbox?.GetHighlightedWords();
                    if (words != null)
                    {
                        List<List<OCR.Word>> highlightedWords = words;
                        ViewModelLocator.Instance.ImageViewer.pictureBox.SetRubberBand(ViewModelLocator.Instance.ImageViewer.pictureBox.GetRubberBand(), true);
                        ViewModelLocator.Instance.Process.rubberBandService.SendRubberBand(highlightedWords);
                    }
                }
                ViewModelLocator.Instance.ImageViewer.pictureBox.SetRubberBand("");
            }
            catch (Exception ex)
            {
                this.logs.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetRubberBand));
            }
        }
        public void ScrollToText(string text)
        {
            var pictureBox = ViewModelLocator.Instance.ImageViewer.pictureBox;

            // Get rubber band rect in image coordinates
            var rect = pictureBox.GetRubberBand();
            if (rect == null || rect.Width == 0 || rect.Height == 0) return;

            // Get current zoom factor
            double zoom = pictureBox.ZoomFactor;

            // Convert image coordinates to display coordinates (multiply by zoom, not divide)
            var displayRect = new System.Drawing.Rectangle(
                (int)Math.Round(rect.X * zoom),
                (int)Math.Round(rect.Y * zoom),
                (int)Math.Round(rect.Width * zoom),
                (int)Math.Round(rect.Height * zoom)
            );

            // Get the panel reference
            var panel = pictureBox.Parent as System.Windows.Forms.ScrollableControl;
            if (panel == null) return;

            // Get viewport dimensions
            var viewport = panel.ClientSize;

            // Get current scroll positions (AutoScrollPosition gives negative values, so negate them)
            int currentScrollX = -panel.AutoScrollPosition.X;
            int currentScrollY = -panel.AutoScrollPosition.Y;


            // Calculate the center of the rectangle in display coordinates
            int rectCenterX = displayRect.X + displayRect.Width / 2;
            int rectCenterY = displayRect.Y + displayRect.Height / 2;

            // Calculate target scroll positions to center the rectangle in viewport
            int targetScrollX = rectCenterX - viewport.Width / 2;
            int targetScrollY = rectCenterY - viewport.Height / 2;

            // Calculate deltas
            int deltaX = targetScrollX - currentScrollX;
            int deltaY = targetScrollY - currentScrollY;

            // Apply the scrolling using your custom methods
            if (deltaX != 0)
                pictureBox.ScrollX(deltaX);
            if (deltaY != 0)
            {
                if (deltaY < 0) deltaY = deltaY * GetMultiplier(zoom);
                pictureBox.ScrollY(deltaY);
            }
        }
        int GetMultiplier(double zoom)
        {
            if (zoom == 1.2) return 5;
            if (zoom == 1.3) return 6;
            if (zoom == 1.7) return 8;
            return zoom > 1 ? 5 : 3;  // fallback multiplier
        }
        //public void ScrollToText(string text)
        //{
        //    var pictureBox = ViewModelLocator.Instance.ImageViewer.pictureBox;

        //    // Get rubber band rect in image coordinates
        //    var rect = pictureBox.GetRubberBand();

        //    if (rect == null || rect.Width == 0 || rect.Height == 0) return;

        //    // Get current zoom factor
        //    double zoom = pictureBox.ZoomFactor;

        //    // Convert image coordinates to scaled coordinates
        //    var scaledRect = new System.Drawing.Rectangle(
        //        (int)Math.Round(rect.X / zoom),
        //        (int)Math.Round(rect.Y / zoom),
        //        (int)Math.Round(rect.Width / zoom),
        //        (int)Math.Round(rect.Height / zoom)
        //    );

        //    // Get the actual panel and picture box references
        //    var panel = pictureBox.Parent as System.Windows.Forms.ScrollableControl;

        //    // Get viewport dimensions (use ClientSize to exclude scrollbar space)
        //    var viewport = panel.ClientSize;

        //    // Get current scroll positions
        //    int currentScrollX = panel.Bounds.X;
        //    int currentScrollY = panel.Bounds.Y;

        //    // Calculate where the rectangle currently appears in the viewport
        //    // (accounting for current PictureBox position)
        //    System.Drawing.Point currentPictureBoxLocation = pictureBox.Location;
        //    int rectViewportX = scaledRect.X + currentPictureBoxLocation.X;
        //    int rectViewportY = scaledRect.Y + currentPictureBoxLocation.Y;

        //    // Calculate target scroll positions to center the rectangle in viewport
        //    int targetScrollX = currentScrollX;
        //    int targetScrollY = currentScrollY;

        //    // Only scroll horizontally if image is wider than viewport
        //    if (currentScrollX != 0)
        //    {
        //        // Calculate how much to scroll to center the rectangle horizontally
        //        int rectCenterX = scaledRect.X + scaledRect.Width;
        //        int viewportCenterX = (int)((viewport.Width / 2) * zoom);

        //        // We want rectCenterX to be at viewportCenterX
        //        // Current picture box X position is: -currentScrollX (from CenterImage logic)
        //        // New scroll value needed: rectCenterX - viewportCenterX

        //        targetScrollX = rectCenterX - (viewportCenterX * 2);
        //    }

        //    // Only scroll vertically if image is taller than viewport  
        //    if (currentScrollY != 0)
        //    {
        //        // Calculate how much to scroll to center the rectangle vertically
        //        int rectCenterY = scaledRect.Y + scaledRect.Height / 2;
        //        int viewportCenterY = viewport.Height / 2;

        //        // We want rectCenterY to be at viewportCenterY
        //        targetScrollY = rectCenterY - (viewportCenterY * 2);
        //    }

        //    // Calculate deltas for your custom ScrollX/ScrollY methods
        //    int deltaX = targetScrollX - currentScrollX;
        //    int deltaY = targetScrollY - currentScrollY;

        //    // Apply the scrolling using your custom methods
        //    if (deltaX != 0)
        //    {
        //        pictureBox.ScrollX(deltaX);
        //    }

        //    if (deltaY != 0)
        //    {
        //        pictureBox.ScrollY(deltaY);
        //    }
        //}
        public void SendRubberBand(List<List<OCR.Word>> highlightedWords)
        {
            // Early validation
            if (!IsValidInput(highlightedWords))
                return;
            try
            {
                var contextType = GetDataContextType();
                var wordCount = highlightedWords.Count;

                // Route to appropriate handler based on context type
                switch (contextType)
                {
                    case DataContextType.Consignee:
                    case DataContextType.Shipper:
                    case DataContextType.BillTo:
                        HandleAccountData(highlightedWords);
                        break;
                    case DataContextType.Items when wordCount > 1:
                        HandleItemsData(highlightedWords);
                        break;

                    case DataContextType.Reference when wordCount > 1:
                        HandleReferenceData(highlightedWords);
                        break;

                    default:
                        HandleSinglePropertyData(highlightedWords);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log the exception appropriately
                logs.ErrorLogToFile($"Error in SendRubberBand: {ex}", ex.StackTrace, nameof(SendRubberBand));
            }
            finally
            {
                FocusTargetElement();
            }
        }
        private bool IsIncompleteAccountData(dynamic accountData)
        {
            return string.IsNullOrEmpty(accountData.name) ||
                   (string.IsNullOrEmpty(accountData.city) &&
                    string.IsNullOrEmpty(accountData.state) &&
                    string.IsNullOrEmpty(accountData.zip));
        }
        private void SetAccountProperties(dynamic accountData)
        {
            var addressData = ProcessAddresses(accountData.addresses);

            // Use a more efficient property setting approach
            var propertyMap = new Dictionary<string, object>
            {
                ["Name"] = accountData.name ?? string.Empty,
                ["Address"] = addressData["address1"],
                ["Address2"] = addressData["address2"],
                ["Address3"] = addressData["address3"],
                ["City"] = accountData.city ?? string.Empty,
                ["State"] = accountData.state ?? string.Empty,
                ["Zip"] = accountData.zip ?? string.Empty
            };

            SetMultipleProperties(propertyMap);
        }
        private bool IsValidInput(List<List<OCR.Word>> highlightedWords)
        {
            return highlightedWords?.Count > 0 &&
                   targetDataContext != null &&
                   !string.IsNullOrWhiteSpace(targetPropertyName);
        }
        private DataContextType GetDataContextType()
        {
            var typeName = targetDataContext.GetType().Name.ToUpperInvariant();

            if (typeName.StartsWith("CONSIGNEE")) return DataContextType.Consignee;
            if (typeName.StartsWith("SHIPPER")) return DataContextType.Shipper;
            if (typeName.StartsWith("BILLTO")) return DataContextType.BillTo;
            if (typeName.StartsWith("INSTRUCTION") || targetPropertyName == "Instruction")
                return DataContextType.Instruction;
            if (typeName.StartsWith("ITEMS") || targetPropertyName == "Items")
                return DataContextType.Items;
            if (typeName.StartsWith("REFERENCE")) return DataContextType.Reference;

            return DataContextType.Unknown;
        }
        private void HandleAccountData(List<List<OCR.Word>> highlightedWords)
        {
            var textBlocks = ConvertWordsToTextBlocks(highlightedWords);
            var accountData = GetAccountPerBlock(new ArrayList(textBlocks));
            if (accountData == null)
            {
                HandleSinglePropertyData(highlightedWords);
                return;
            }

            if (IsIncompleteAccountData(accountData))
            {
                HandleSinglePropertyData(highlightedWords);
                return;
            }

            SetAccountProperties(accountData);


        }
        private void HandleReferenceData(List<List<OCR.Word>> highlightedWords)
        {
            var textBlocks = highlightedWords
                .Select(wordList => string.Join(",", wordList.Select(word => word.text)))
                .ToList();

            var references = GetReferences(textBlocks);
            if (references?.Any() != true)
                return;

            var referenceCode = GetReferenceCodeFromUser();
            if (string.IsNullOrEmpty(referenceCode))
                return;

            AddReferencesToCollection(references, referenceCode);
        }
        private void HandleItemsData(List<List<OCR.Word>> highlightedWords)
        {
            HandleSinglePropertyData(highlightedWords);
        }
        private void AddReferencesToCollection(IEnumerable<dynamic> references, string code)
        {
            var referenceCollection = ViewModelLocator.Instance.Reference?.Reference;
            if (referenceCollection == null)
                return;

            // Remove last item if collection is not empty
            if (referenceCollection.Count > 0)
            {
                referenceCollection.RemoveAt(referenceCollection.Count - 1);
            }

            foreach (var reference in references)
            {
                referenceCollection.Add(new ReferenceInfo
                {
                    code = code,
                    description = reference.description ?? string.Empty
                });
            }
        }
        private string GetReferenceCodeFromUser()
        {
            var inputBox = new CustomInputBox(Application.Current.MainWindow)
            {
                Caption = "Code",
                Message = "Please enter the code for references."
            };

            return inputBox.ShowDialog() == SweetAlertSharp.Enums.SweetAlertResult.YES
                ? inputBox._Content.Text.Trim()
                : null;
        }
        private void HandleSinglePropertyData(List<List<OCR.Word>> highlightedWords)
        {
            var cleanText = ReturnAsSingle(highlightedWords);
            SetPropertyValue(targetPropertyName, cleanText);
        }
        private List<string> ConvertWordsToTextBlocks(List<List<OCR.Word>> highlightedWords)
        {
            return highlightedWords
                .SelectMany(wordList => wordList) // Flatten all words
                .OrderBy(word => word.lineNumber) // Order by lineNumber
                .GroupBy(word => word.lineIndex) // Group by lineIndex
                .OrderBy(group => group.Key) // Order by lineIndex to maintain reading order
                .Select(group => string.Join(" ", group.Select(word => word.text)))
                .ToList();
        }
        private void SetPropertyValue(string propertyName, object value)
        {
            try
            {
                var property = GetCachedProperty(propertyName);
                property?.SetValue(targetDataContext, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set property '{propertyName}': {ex.Message}", ex);
            }
        }
        private void SetMultipleProperties(Dictionary<string, object> propertyValues)
        {
            foreach (var kvp in propertyValues)
            {
                try
                {
                    SetPropertyValue(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    // Log individual property errors but continue with others
                    System.Diagnostics.Debug.WriteLine($"Failed to set property '{kvp.Key}': {ex.Message}");
                }
            }
        }
        private PropertyInfo GetCachedProperty(string propertyName)
        {
            var cacheKey = $"{targetDataContext.GetType().FullName}.{propertyName}";
            return PropertyCache.GetOrAdd(cacheKey, _ =>
                targetDataContext.GetType().GetProperty(propertyName));
        }
        private Dictionary<string, string> ProcessAddresses(IList<string> addresses)
        {
            if (addresses?.Count > 2)
            {
                return new Dictionary<string, string>
                {
                    { "address1", addresses.First() },
                    { "address2", addresses[1] },
                    { "address3", string.Join(" ", addresses.Skip(2)) }
                };
            }else if (addresses?.Count == 2)
            {
                return new Dictionary<string, string>
                {
                    { "address1", addresses.First() },
                    { "address2", addresses[1] },
                    { "address3", string.Empty }
                };
            }
            else if (addresses?.Count == 1)
            {
                return new Dictionary<string, string>
                {
                    { "address1", addresses.First() },
                    { "address2", string.Empty },
                    { "address3", string.Empty }
                };
            }

            return new Dictionary<string, string>
                {
                    { "address1", string.Empty },
                    { "address2", string.Empty },
                    { "address3", string.Empty }
                };
        }
        public void SetTarget(object source, object dataContext, string propertyName)
        {
            targetDataContext = dataContext;
            targetPropertyName = propertyName;
            targetSource = source;
        }
        public AccountInfo GetAccountPerBlock(ArrayList Arr)
        {
            AccountInfo Info = new AccountInfo();
            Info.addresses = new List<string>();
            List<string> rawData = new List<string>(Arr.Cast<string>());

            // Check if this is a "Valid Account" format - handle it specially
            if (rawData.Count == 1)
            {
                var temp_info = ParseOneLineFormat(rawData);
                if (temp_info != null)
                {
                    return temp_info;
                }
            }

            // Create a working copy for processing
            List<string> workingData = rawData.Select(x => x.ToUpper().Trim()).ToList();
            List<bool> processed = new List<bool>(new bool[workingData.Count]);

            // Step 1: Extract Company Name (usually first non-empty line)
            if (workingData.Count > 0)
            {
                // Start with the first line
                Info.name = this.CleanGarbageWords(rawData[0]);
                processed[0] = true;

                // Check if second line is part of the name (not an address/city/state/zip pattern)
                if (workingData.Count > 1)
                {
                    string secondLine = rawData[1];

                    // Check if the second line is likely part of the company name
                    // (not starting with common address keywords or containing address patterns)
                    if (this.IsLikelyCompanyName(secondLine))
                    {
                        Info.name += " " + this.CleanGarbageWords(secondLine);
                        processed[1] = true;
                    }
                }

                Info.name = RemoveDoubleSpace(Info.name);
            }

            // Step 2: Extract City/State/Zip (look for patterns)
            for (int i = 0; i < workingData.Count; i++)
            {
                if (processed[i]) continue;

                string line = workingData[i];
                CityStateZipInfo tmpCSZ = this.GetCityStateZip(CleanGarbageWords(rawData[i]));
                if (tmpCSZ.hasValue)
                {
                    Info.city = this.CleanStringKeepSingleSpace(tmpCSZ.City?.Trim());
                    Info.state = this.CleanStringKeepSingleSpace(tmpCSZ.State?.Trim());
                    Info.zip = this.CleanStringKeepSingleSpace(tmpCSZ.Zip.Trim());
                    Info.code = this.CleanStringKeepSingleSpace(tmpCSZ.CountryCode?.Trim());

                    if (IsAddressPattern(line))
                        workingData[i] = RemoveDoubleSpace(RemoveSpecialCharacters(workingData[i]
                           .Replace(tmpCSZ?.City.ToUpper(), "")
                           .Replace(tmpCSZ?.State.ToUpper(), "")
                           .Replace(tmpCSZ?.Zip.ToUpper(), "")
                           .Replace(tmpCSZ?.CountryCode.ToUpper(), "")
                           .Trim()));
                    else
                        processed[i] = true;
                    break;
                }
            }

            // Step 3: Extract Phone Numbers and Associated Contact Names
            for (int i = 0; i < workingData.Count; i++)
            {
                if (processed[i]) continue;

                string phone = GetPhone(rawData[i]);
                if (!string.IsNullOrEmpty(phone))
                {
                    Info.contact = phone;

                    // Extract contact name from the same line
                    string remainingText = rawData[i].Replace(phone, "").Trim();
                    remainingText = this.CleanContactName(remainingText);

                    if (!string.IsNullOrEmpty(remainingText) && this.IsValidContactName(remainingText))
                        Info.contact = this.CleanGarbageWords(remainingText);

                    processed[i] = true;
                    break;
                }
            }

            // Step 4: Extract Contact Names (if not found with phone)
            if (string.IsNullOrEmpty(Info.contact))
            {
                for (int i = 0; i < workingData.Count; i++)
                {
                    if (processed[i]) continue;

                    string line = workingData[i];
                    if (line.Contains("ATTN") || line.Contains("CONTACT") || line.Contains("ATTENTION"))
                    {
                        // Extract contact name from attention line
                        string contactName = ExtractContactFromAttentionLine(rawData[i]);

                        if (IsValidContactName(contactName))
                        {
                            Info.contact = contactName;
                            processed[i] = false;
                            break;
                        }
                    }
                }
            }

            // Step 5: Extract Addresses (everything else that looks like an address)
            for (int i = 0; i < workingData.Count; i++)
            {

                string temp_address = workingData[i].ToUpper();
                if (IsEmail(temp_address)) processed[i] = true; // Skip emails
                if (temp_address.Equals("UNITED STATES", StringComparison.OrdinalIgnoreCase))
                    processed[i] = true; // Skip country lines

                if (processed[i]) continue;

                // Skip if it looks like company name, phone, or contact info
                if (IsLikelyCompanyName(temp_address) || RegexValidation(temp_address, regex.regexPatterns.PHONE))
                    continue;

                if (temp_address.Contains("ATTENTION") ||
                    temp_address.Contains("ATTN") ||
                    temp_address.Contains("TO :") ||
                    temp_address.Contains("TO:"))
                {
                    string extractedAddress = ExtractAddressFromLine(temp_address);
                    if (!string.IsNullOrEmpty(extractedAddress) && IsValidAddress(extractedAddress))
                    {
                        Info.addresses.Add(extractedAddress);
                        processed[i] = true;
                        continue;
                    }
                }

                // Check if it's a standard address pattern
                if (RegexValidation(temp_address, regex.regexPatterns.ADDRESS) ||
                    temp_address.Contains(" CO ") || IsAddressPattern(temp_address))
                {
                    string cleanAddress = ExtractAddressFromLine(temp_address);
                    if (!string.IsNullOrEmpty(cleanAddress))
                    {
                        // Look ahead for potential address continuation lines
                        List<string> addressParts = new List<string> { cleanAddress };
                        int nextIndex = i + 1;

                        // Check next few lines for address continuations
                        while (nextIndex < workingData.Count && nextIndex <= i + 3) // Look ahead max 3 lines
                        {
                            if (processed[nextIndex] || IsEmail(workingData[nextIndex]))
                            {
                                nextIndex++;
                                continue;
                            }
                            if (!processed[nextIndex] && workingData[nextIndex].Equals("UNITED STATES", StringComparison.OrdinalIgnoreCase))
                            {
                                processed[nextIndex] = true; // Skip country lines
                                nextIndex++;
                                continue;
                            }

                            string nextLine = workingData[nextIndex].Trim();
                            string nextUpperLine = workingData[nextIndex];

                            // Skip obvious non-address lines
                            if (IsLikelyCompanyName(nextUpperLine) ||
                                RegexValidation(nextUpperLine, regex.regexPatterns.PHONE) ||
                                string.IsNullOrEmpty(nextLine))
                            {
                                nextIndex++;
                                continue;
                            }

                            // Check if it's an address continuation
                            if (IsAddressContinuation(nextLine, cleanAddress))
                            {
                                addressParts.Add(RemoveDoubleSpace(this.CleanGarbageWords(nextLine)));
                                processed[nextIndex] = true; // Mark as processed
                            }
                            else
                            {
                                break; // Stop looking if we hit a non-continuation
                            }

                            nextIndex++;
                        }

                        // Combine all address parts
                        string finalAddress = string.Join(" ", addressParts);
                        Info.addresses.Add(RemoveDoubleSpace(finalAddress));
                        processed[i] = true;
                    }
                }

            }

            // Step 6: Handle remaining unprocessed lines as potential addresses
            List<string> unprocessedLines = new List<string>();
            for (int i = 0; i < workingData.Count; i++)
            {
                if (!processed[i] && !string.IsNullOrEmpty(workingData[i].Trim()))
                {

                    if (!this.IsValidContactName(workingData[i]))
                    {
                        unprocessedLines.Add(this.CleanGarbageWords(workingData[i]));
                    }
                }
            }

            // Combine related address lines
            if (unprocessedLines.Count > 0)
            {
                string combinedAddress = string.Join(", ", unprocessedLines);
                if (!string.IsNullOrEmpty(combinedAddress))
                {
                    Info.addresses.Add(RemoveDoubleSpace(this.CleanGarbageWords(combinedAddress)));
                }
            }

            return Info;
        }
    }
}
