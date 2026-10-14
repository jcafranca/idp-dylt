using DDCInteligence.Utilities.Helpers;
using DotNetCS;
using System.Collections;
using System.Collections.Generic;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.DataTypes
{
    public interface IRubberBand
    {
        void SetFocus(object parent, object source);
        void GetRubberBand(RubberBandFetched message);
        void SendRubberBand(List<List<OCR.Word>> highlightedWords);
        void ScrollToText(string value);
        void SetTarget(object source, object dataContext, string propertyName);
        void FocusTargetElement();
        AccountInfo GetAccountPerBlock(ArrayList Arr);
        List<InstructionInfo> GetInstruction(IEnumerable<string> instructions);
    }
}
