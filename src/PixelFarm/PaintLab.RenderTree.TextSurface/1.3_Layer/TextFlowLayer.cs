﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing;
using LayoutFarm.RenderBoxes;
namespace LayoutFarm.TextEditing
{
    class TextMarkerLayer
    {
        public TextMarkerLayer(List<VisualMarkerSelectionRange> visualMarkers)
        {
            VisualMarkers = visualMarkers;
        }
        internal List<VisualMarkerSelectionRange> VisualMarkers { get; private set; }
        internal int VisualMarkerCount => (VisualMarkers == null) ? 0 : VisualMarkers.Count;
        public void AddMarker(VisualMarkerSelectionRange markerSelectionRange)
        {
            VisualMarkers.Add(markerSelectionRange);
        }
        public void Clear()
        {
            VisualMarkers.Clear();
        }
        public void Remove(VisualMarkerSelectionRange markerRange)
        {
            VisualMarkers.Remove(markerRange);
        }
    }

    public enum LineHeightHint : byte
    {
        SameLineHeight,
        VaryLineHeight
    }

    sealed class TextFlowLayer
    {
        //TextFlowLayer: contains and manages collection of TextLineBox
        //public event EventHandler Reflow; //TODO: review this field

        public event EventHandler ContentSizeChanged;//TODO: review this field 

        //TODO: use linked-list or tree for lines??
        List<TextLineBox> _lines = new List<TextLineBox>();
        ITextFlowLayerOwner _owner;

        public TextFlowLayer(ITextFlowLayerOwner owner, RunStyle defaultSpanStyle)
        {
            _owner = owner;

            //start with single line per layer
            //and can be changed to multiline
            DefaultRunStyle = defaultSpanStyle;

            //add default lines
            _lines.Add(new TextLineBox(this));

            VisualLineOverlapped = true;
        }
        public LineHeightHint LineHeightHint { get; set; } //help on hit test, find line from y pos

        /// <summary>
        /// visual output of each line may overlap each other
        /// </summary>
        public bool VisualLineOverlapped { get; set; }
        public int OwnerWidth => _owner.Width;

        public RunStyle DefaultRunStyle { get; private set; }

        public void SetDefaultRunStyle(RunStyle runStyle)
        {
            DefaultRunStyle = runStyle;
        }
        internal void ClientLineBubbleupInvalidateArea(Rectangle clientInvalidatedArea)
        {
            _owner.ClientLayerBubbleUpInvalidateArea(clientInvalidatedArea);
        }

        public int DefaultLineHeight => DefaultRunStyle.ResolvedFont.LineSpacingInPixels;

        internal void NotifyContentSizeChanged() => ContentSizeChanged?.Invoke(this, EventArgs.Empty);

        internal Run LatestHitRun { get; set; }

        internal IEnumerable<Run> GetDrawingIter(Run start, Run stop)
        {
            List<TextLineBox> lines = _lines;
            int j = lines.Count;
            for (int i = 0; i < j; ++i)
            {
                LinkedListNode<Run> curNode = lines[i].Last;
                bool enableIter = false;
                while (curNode != null)
                {
                    Run editableRun = curNode.Value;
                    if (editableRun == stop)
                    {
                        //found stop
                        enableIter = true;
                        yield return editableRun;

                        if (stop == start)
                        {
                            break;//break here
                        }
                        curNode = curNode.Previous;
                        continue;//get next
                    }
                    else if (editableRun == start)
                    {
                        //stop
                        yield return editableRun;
                        break;
                    }
                    //
                    //
                    if (enableIter)
                    {
                        yield return editableRun;
                    }
                    curNode = curNode.Previous;
                }
            }
        }

        public int LineCount => _lines.Count;

        public void AcceptVisitor(RunVisitor visitor)
        {
            //similar to Draw...
            List<TextLineBox> lines = _lines;
            int renderAreaTop;
            int renderAreaBottom;
            if (visitor.UseUpdateArea)
            {
                renderAreaTop = visitor.UpdateArea.Top;
                renderAreaBottom = visitor.UpdateArea.Bottom;
            }
            else
            {
                renderAreaTop = 0;
                renderAreaBottom = this.Bottom;
            }


            bool foundFirstLine = false;
            int j = lines.Count;
            for (int i = 0; i < j; ++i)
            {
                TextLineBox line = lines[i];
                int y = line.Top;

                if (visitor.StopOnNextLine)
                {
                    break; //break from for loop=> go to end
                }

                visitor.VisitNewLine(y); //*** 

                if (!visitor.SkipCurrentLineEditableRunIter)
                {
                    LinkedListNode<Run> curNode = line.First;
                    if (!foundFirstLine)
                    {
                        if (y + line.ActualLineHeight < renderAreaTop)
                        {
                            continue;
                        }
                        else
                        {
                            foundFirstLine = true;
                        }
                    }
                    else
                    {
                        if (VisualLineOverlapped)
                        {
                            if (i < j - 1)
                            {
                                //check next line
                                TextLineBox lowerLine = lines[i + 1];
                                if (lowerLine.Top > y)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (y > renderAreaBottom)
                            {
                                break;
                            }
                        }
                    }

                    while (curNode != null)
                    {
                        Run child = curNode.Value;

                        //iter entire line, not check horizontal line intersect
                        visitor.VisitEditableRun(child);

                        curNode = curNode.Next;
                    }
                }
            }
        }

        internal void DrawChildContentLcdEffectText(DrawBoard d, UpdateArea updateArea, VisualSelectionRange selRange)
        {

            List<TextLineBox> lines = _lines;
            int renderAreaTop = updateArea.Top;
            int renderAreaBottom = updateArea.Bottom;
            bool foundFirstLine = false;
            int j = lines.Count;

            int enter_canvasX = d.OriginX;
            int enter_canvasY = d.OriginY;

            Rectangle currentClip = d.CurrentClipRect;
            Color prev_colorHint = d.TextBackgroundColorHint;


            for (int i = 0; i < j; ++i)
            {
                //draw textline, along with visual selection range

                TextLineBox line = lines[i];
                int linetop = line.Top;

                if (!foundFirstLine)
                {
                    if (linetop + line.ActualLineHeight < renderAreaTop)
                    {
                        continue;
                    }
                    else
                    {
                        foundFirstLine = true;
                    }
                }
                else
                {
                    if (linetop > renderAreaBottom)
                    {

                        if (VisualLineOverlapped)
                        {
                            //more check
                            if (i < j - 1)
                            {
                                //check next line
                                TextLineBox lowerLine = lines[i + 1];
                                if (lowerLine.Top - lowerLine.OverlappedTop > linetop)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                updateArea.OffsetY(-linetop); //offset 

                switch (selRange.GetLineClip(i, out int clipLeft, out int clipWidth))
                {
                    default: throw new NotSupportedException();
                    case VisualSelectionRange.ClipRectKind.No: break;
                    case VisualSelectionRange.ClipRectKind.SameLine:
                    case VisualSelectionRange.ClipRectKind.StartLine:
                        {

                            //[A]: normal line
                            LinkedListNode<Run> curLineNode = line.First;
                            while (curLineNode != null)
                            {
                                Run run = curLineNode.Value;
                                if (run.HitTest(updateArea))
                                {
                                    int x = run.Left;

                                    d.SetCanvasOrigin(enter_canvasX + x, enter_canvasY + linetop);
                                    updateArea.OffsetX(-x);

                                    run.Draw(d, updateArea);
                                    //-----------
                                    updateArea.OffsetX(x);
                                }
                                curLineNode = curLineNode.Next;
                            }
                            d.SetCanvasOrigin(enter_canvasX, enter_canvasY + linetop);//restore 
                            //--------------------------------
                            //[B] selection part                             
                            curLineNode = line.First; //begin at first run again
                            d.TextBackgroundColorHint = selRange.BackgroundColor; //selection color, TODO: review this again                             
                            d.SetClipRect(new Rectangle(clipLeft, 0, clipWidth, line.ActualLineHeight));
                            while (curLineNode != null)
                            {
                                Run run = curLineNode.Value;
                                if (run.HitTest(updateArea))
                                {
                                    int x = run.Left;

                                    d.SetCanvasOrigin(enter_canvasX + x, enter_canvasY + linetop);
                                    updateArea.OffsetX(-x);
                                    d.CurrentTextColor = selRange.FontColor;
                                    run.Draw(d, updateArea);
                                    //-----------
                                    updateArea.OffsetX(x);
                                }
                                curLineNode = curLineNode.Next;
                            }
                        }
                        break;
                    case VisualSelectionRange.ClipRectKind.InBetween:
                        {
                            //similar to [A] but we use color hint = selRange.BackgroundColor
                            d.SetCanvasOrigin(enter_canvasX, enter_canvasY + linetop);//restore 
                            d.SetClipRect(new Rectangle(0, 0, OwnerWidth, line.ActualLineHeight));//set clip relative to latest canvas origin
                            d.TextBackgroundColorHint = selRange.BackgroundColor;//

                            LinkedListNode<Run> curLineNode = line.First;
                            while (curLineNode != null)
                            {
                                Run run = curLineNode.Value;
                                if (run.HitTest(updateArea))
                                {
                                    int x = run.Left;

                                    d.SetCanvasOrigin(enter_canvasX + x, enter_canvasY + linetop);
                                    updateArea.OffsetX(-x);

                                    run.Draw(d, updateArea);
                                    //-----------
                                    updateArea.OffsetX(x);
                                }
                                curLineNode = curLineNode.Next;
                            }
                            d.SetCanvasOrigin(enter_canvasX, enter_canvasY + linetop);//restore 
                            //--------------------------------
                        }
                        break;
                    case VisualSelectionRange.ClipRectKind.EndLine:
                        {
                            //[A] normal bg
                            d.SetCanvasOrigin(enter_canvasX, enter_canvasY + linetop);//restore 
                            d.SetClipRect(new Rectangle(0, 0, OwnerWidth, line.ActualLineHeight));//set clip relative to latest canvas origin
                            d.TextBackgroundColorHint = prev_colorHint;//normal bg

                            LinkedListNode<Run> curLineNode = line.First;
                            while (curLineNode != null)
                            {
                                Run run = curLineNode.Value;
                                if (run.HitTest(updateArea))
                                {
                                    int x = run.Left;

                                    d.SetCanvasOrigin(enter_canvasX + x, enter_canvasY + linetop);
                                    updateArea.OffsetX(-x);

                                    run.Draw(d, updateArea);
                                    //-----------
                                    updateArea.OffsetX(x);
                                }
                                curLineNode = curLineNode.Next;
                            }
                            d.SetCanvasOrigin(enter_canvasX, enter_canvasY + linetop);//restore 
                            //--------------------------------
                            //[B] selection part                             
                            curLineNode = line.First; //begin at first run again
                            d.TextBackgroundColorHint = selRange.BackgroundColor; //selection color, TODO: review this again                             
                            d.SetClipRect(new Rectangle(0, 0, clipWidth, line.ActualLineHeight));
                            while (curLineNode != null)
                            {
                                Run run = curLineNode.Value;
                                if (run.HitTest(updateArea))
                                {
                                    int x = run.Left;

                                    d.SetCanvasOrigin(enter_canvasX + x, enter_canvasY + linetop);
                                    updateArea.OffsetX(-x);
                                    d.CurrentTextColor = selRange.FontColor;
                                    run.Draw(d, updateArea);
                                    //-----------
                                    updateArea.OffsetX(x);
                                }
                                curLineNode = curLineNode.Next;
                            }
                        }
                        break;
                }


                //
                d.SetCanvasOrigin(enter_canvasX, enter_canvasY);

                updateArea.OffsetY(linetop); //restore 
            }

            d.SetCanvasOrigin(enter_canvasX, enter_canvasY);
            d.SetClipRect(new Rectangle(enter_canvasX, enter_canvasY, currentClip.Width, currentClip.Height));
            d.TextBackgroundColorHint = prev_colorHint;
        }

        internal void DrawChildContent(DrawBoard d, UpdateArea updateArea)
        {

            //this.BeginDrawingChildContent(); 

            List<TextLineBox> lines = _lines;
            int renderAreaTop = updateArea.Top;
            int renderAreaBottom = updateArea.Bottom;
            bool foundFirstLine = false;
            int j = lines.Count;

            int enter_canvasX = d.OriginX;
            int enter_canvasY = d.OriginY;

            for (int i = 0; i < j; ++i)
            {
                TextLineBox line = lines[i];

                int lineTop = line.Top;

                if (!foundFirstLine)
                {
                    if (lineTop + line.ActualLineHeight < renderAreaTop)
                    {
                        continue;
                    }
                    else
                    {
                        foundFirstLine = true;
                    }
                }
                else
                {
                    if (lineTop > renderAreaBottom)
                    {
                        if (VisualLineOverlapped)
                        {
                            //more check
                            if (i < j - 1)
                            {
                                //check next line
                                TextLineBox lowerLine = lines[i + 1];
                                if (lowerLine.Top - lowerLine.OverlappedTop > lineTop)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                updateArea.OffsetY(-lineTop); //offset

                LinkedListNode<Run> curLineNode = line.First;

                while (curLineNode != null)
                {
                    Run run = curLineNode.Value;
                    if (run.HitTest(updateArea))
                    {
                        int x = run.Left;

                        d.SetCanvasOrigin(enter_canvasX + x, enter_canvasY + lineTop);
                        updateArea.OffsetX(-x);

                        run.Draw(d, updateArea);
                        //-----------
                        updateArea.OffsetX(x);
                    }

                    curLineNode = curLineNode.Next;
                }

                updateArea.OffsetY(lineTop); //restore
            }

            d.SetCanvasOrigin(enter_canvasX, enter_canvasY);
        }

        public bool HitTestCore(HitChain hitChain)
        {
#if DEBUG
            //if (hitChain.dbugHitPhase == dbugHitChainPhase.MouseDown)
            //{

            //}
#endif

            //line must be arranged

            List<TextLineBox> lines = _lines;
            int j = lines.Count;
            int y = hitChain.TestPoint.Y;

            for (int i = 0; i < j; ++i)
            {
                TextLineBox line = lines[i];
                if (line.LineBottom < y)
                {
                    continue;
                }
                else if (line.LineTop > y)
                {
                    return false;
                }
                else if (line.HitTestCore(hitChain))
                {
                    return true;
                }

            }
            return false;
        }

        int _posCalContentW;
        int _posCalContentH;
        public void TopDownReCalculateContentSize()
        {
#if DEBUG

            //vinv_dbug_EnterLayerReCalculateContent(this);
#endif

            //TODO: review this again!
            TextLineBox lastline = _lines[_lines.Count - 1];
            _posCalContentW = lastline.ActualLineWidth;//?  TODO: review this, use max width, not the width of last line
            _posCalContentH = lastline.ActualLineHeight + lastline.LineTop;

            //SetPostCalculateLayerContentSize(lastline.ActualLineWidth, lastline.ActualLineHeight + lastline.LineTop);
#if DEBUG
            //vinv_dbug_ExitLayerReCalculateContent();
#endif
        }

        public int Bottom
        {
            //get bottom
            get
            {
                TextLineBox lastLine = GetTextLine(this.LineCount - 1);
                return (lastLine != null) ? lastLine.Top + lastLine.ActualLineHeight : DefaultLineHeight;
            }
        }

        internal TextLineBox GetTextLine(int lineId) => (lineId < _lines.Count) ? _lines[lineId] : null;

        internal TextLineBox GetTextLineAtPos(int y)
        {
            List<TextLineBox> lines = _lines;
            if (lines != null)
            {
                if (LineHeightHint == LineHeightHint.SameLineHeight)
                {
                    if (y >= 0 && lines.Count > 0)
                    {
                        //same line height 
                        //if (this.DefaultLineHeight == 0)
                        //{

                        //}
                        //calculate 
                        //TODO: check interlinespace > 0 
                        int index = y / this.DefaultLineHeight;
                        if (index < lines.Count)
                        {
                            return lines[index];
                        }
                        else
                        {
                            return lines[lines.Count - 1];//last line
                        }
                    }
                }
                else
                {
                    //vary, 
                    //TODO: use HeightTree?

                    int j = lines.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        TextLineBox line = lines[i];
                        if (line.LineBottom < y)
                        {
                            continue;
                        }
                        else if (line.LineTop > y)
                        {
                            break;
                        }
                        else if (line.IntersectsWith(y))
                        {
                            return line;
                        }
                    }
                }

            }
            return null;
        }
        /// <summary>
        /// append to last
        /// </summary>
        /// <param name="line"></param>
        void AppendLine(TextLineBox line)
        {
            List<TextLineBox> lines = _lines;
            int lineCount = lines.Count;
            TextLineBox lastLine = lines[lineCount - 1];
            line.SetLineNumber(lineCount);
            line.SetTop(lastLine.Top + lastLine.ActualLineHeight);
            lines.Add(line);
        }

        internal TextLineBox InsertNewLine(int insertAt)
        {
            TextLineBox newLine = new TextLineBox(this);
            this.InsertLine(insertAt, newLine);
            return newLine;
        }
        void InsertLine(int insertAt, TextLineBox textLine)
        {
            if (insertAt < 0)
            {
                throw new NotSupportedException();
            }
            List<TextLineBox> lines = _lines;
            int j = lines.Count;
            if (insertAt >= j)
            {
                AppendLine(textLine);
            }
            else
            {
                TextLineBox line = lines[insertAt];
                int cy = line.Top;
                textLine.SetTop(cy);
                textLine.SetLineNumber(insertAt);
                cy += line.ActualLineHeight;
                for (int i = insertAt; i < j; i++)
                {
                    line = lines[i];
                    line.SetTop(cy);
                    line.SetLineNumber(i + 1);
                    cy += line.ActualLineHeight;
                }

                lines.Insert(insertAt, textLine);
            }
        }

        public void CopyContentToStringBuilder(StringBuilder stBuilder)
        {
            List<TextLineBox> lines = _lines;
            int j = lines.Count;
            for (int i = 0; i < j; ++i)
            {
                if (i > 0)
                {
                    //TODO: review => preserve line ending char or not 
                    stBuilder.AppendLine();
                }
                lines[i].CopyLineContent(stBuilder);
            }
        }

        internal IEnumerable<Run> TextRunForward(Run startRun, Run stopRun)
        {
            TextLineBox currentLine = startRun.OwnerLine;
            TextLineBox stopLine = stopRun.OwnerLine;
            if (currentLine == stopLine)
            {
                foreach (Run r in currentLine.GetRunIterForward(startRun, stopRun))
                {
                    yield return r;
                }
            }
            else
            {
                foreach (Run r in currentLine.GetRunIterForward(startRun))
                {
                    yield return r;
                }
                currentLine = currentLine.Next;
                while (currentLine != null)
                {
                    if (currentLine == stopLine)
                    {
                        foreach (Run r in currentLine.GetRunIter())
                        {
                            if (r == stopRun)
                            {
                                break;
                            }
                            else
                            {
                                yield return r;
                            }
                        }
                        break;
                    }
                    else
                    {
                        foreach (Run r in currentLine.GetRunIter())
                        {
                            yield return r;
                        }
                        currentLine = currentLine.Next;
                    }
                }
            }
        }
        public void Clear()
        {
            List<TextLineBox> lines = _lines;
            for (int i = lines.Count - 1; i > -1; --i)
            {
                TextLineBox line = lines[i];
                line.RemoveOwnerFlowLayer();
                line.Clear();
            }
            lines.Clear();

            //auto add first line
            _lines.Add(new TextLineBox(this));
        }

        internal void Remove(int lineId)
        {
#if DEBUG
            if (lineId < 0)
            {
                throw new NotSupportedException();
            }
#endif
            //if ((_layerFlags & FLOWLAYER_HAS_MULTILINE) == 0)
            //{
            //    return;
            //}
            List<TextLineBox> lines = _lines;
            if (lines.Count < 2)
            {
                return;
            }

            TextLineBox removedLine = lines[lineId];
            int cy = removedLine.Top;
            //
            lines.RemoveAt(lineId);
            removedLine.RemoveOwnerFlowLayer();

            //arrange 
            int j = lines.Count;
            for (int i = lineId; i < j; ++i)
            {
                TextLineBox line = lines[i];
                line.SetTop(cy);
                line.SetLineNumber(i);
                cy += line.ActualLineHeight;
            }
        }

#if DEBUG
        //void debug_RecordLineInfo(RenderBoxBase owner, EditableTextLine line)
        //{
        //    RootGraphic visualroot = this.dbugVRoot;
        //    if (visualroot.dbug_RecordDrawingChain)
        //    {
        //    }
        //}

        //public override void dbug_DumpElementProps(dbugLayoutMsgWriter writer)
        //{
        //    writer.Add(new dbugLayoutMsg(
        //        this, this.ToString()));
        //    writer.EnterNewLevel();
        //    foreach (EditableRun child in this.dbugGetDrawingIter2())
        //    {
        //        child.dbug_DumpVisualProps(writer);
        //    }
        //    writer.LeaveCurrentLevel();
        //}
        //public override string ToString()
        //{
        //    return "editable flow layer " + "(L" + dbug_layer_id + this.dbugLayerState + ") postcal:" +
        //        this.PostCalculateContentSize.ToString() + " of " + this.OwnerRenderElement.dbug_FullElementDescription();
        //}
        public IEnumerable<Run> dbugGetDrawingIter2()
        {

            List<TextLineBox> lines = _lines;
            int j = lines.Count;
            for (int i = 0; i < j; ++i)
            {
                LinkedListNode<Run> curNode = lines[i].First;
                while (curNode != null)
                {
                    yield return curNode.Value;
                    curNode = curNode.Next;
                }
            }
        }
#endif
    }



}