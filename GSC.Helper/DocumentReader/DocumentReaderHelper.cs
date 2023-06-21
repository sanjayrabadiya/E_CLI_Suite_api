using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Helper.DocumentReader
{
    public class AbstractList
    {
        public int abstractListId { get; set; }
        public List<Level> levels { get; set; }
    }

    public class Background
    {
        public string color { get; set; }
    }

    public class Block
    {
        public CharacterFormat characterFormat { get; set; }
        public ParagraphFormat paragraphFormat { get; set; }
        public List<Inline> inlines { get; set; }
        public List<Row> rows { get; set; }
        public object title { get; set; }
        public object description { get; set; }
        public TableFormat tableFormat { get; set; }
    }

    public class Borders
    {
        public Left left { get; set; }
        public Right right { get; set; }
        public Top top { get; set; }
        public Bottom bottom { get; set; }
        public Vertical vertical { get; set; }
        public Horizontal horizontal { get; set; }
        public DiagonalDown diagonalDown { get; set; }
        public DiagonalUp diagonalUp { get; set; }
    }

    public class Bottom
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Cell
    {
        public List<Block> blocks { get; set; }
        public CellFormat cellFormat { get; set; }
    }

    public class CellFormat
    {
        public int columnSpan { get; set; }
        public int rowSpan { get; set; }
        public double preferredWidth { get; set; }
        public string preferredWidthType { get; set; }
        public string verticalAlignment { get; set; }
        public bool isSamePaddingAsTable { get; set; }
        public Borders borders { get; set; }
        public double cellWidth { get; set; }
        public Shading shading { get; set; }
    }

    public class CharacterFormat
    {
        public string fontColor { get; set; }
        public bool? bold { get; set; }
        public double? fontSize { get; set; }
        public string fontFamily { get; set; }
        public double fontSizeBidi { get; set; }
        public string fontFamilyBidi { get; set; }
        public bool? boldBidi { get; set; }
        public string underline { get; set; }
        public bool? italic { get; set; }
    }

    public class ContinuationNotice
    {
        public List<object> inlines { get; set; }
    }

    public class ContinuationSeparator
    {
        public CharacterFormat characterFormat { get; set; }
        public ParagraphFormat paragraphFormat { get; set; }
        public List<Inline> inlines { get; set; }
    }

    public class DiagonalDown
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class DiagonalUp
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Endnotes
    {
        public List<Separator> separator { get; set; }
        public List<ContinuationSeparator> continuationSeparator { get; set; }
        public List<ContinuationNotice> continuationNotice { get; set; }
    }

    public class EvenFooter
    {
        public List<Block> blocks { get; set; }
    }

    public class EvenHeader
    {
        public List<Block> blocks { get; set; }
    }

    public class FillFormat
    {
        public string color { get; set; }
        public bool fill { get; set; }
    }

    public class FirstPageFooter
    {
        public List<Block> blocks { get; set; }
    }

    public class FirstPageHeader
    {
        public List<Block> blocks { get; set; }
    }

    public class Footer
    {
        public List<Block> blocks { get; set; }
    }

    public class Footnotes
    {
        public List<Separator> separator { get; set; }
        public List<ContinuationSeparator> continuationSeparator { get; set; }
        public List<ContinuationNotice> continuationNotice { get; set; }
    }

    public class Header
    {
        public List<Block> blocks { get; set; }
    }

    public class HeadersFooters
    {
        public Header header { get; set; }
        public Footer footer { get; set; }
        public EvenHeader evenHeader { get; set; }
        public EvenFooter evenFooter { get; set; }
        public FirstPageHeader firstPageHeader { get; set; }
        public FirstPageFooter firstPageFooter { get; set; }
    }

    public class Horizontal
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Inline
    {
        public string text { get; set; }
        public CharacterFormat characterFormat { get; set; }
        public int? shapeId { get; set; }
        public bool? visible { get; set; }
        public double? width { get; set; }
        public double? height { get; set; }
        public double? widthScale { get; set; }
        public double? heightScale { get; set; }
        public LineFormat lineFormat { get; set; }
        public FillFormat fillFormat { get; set; }
        public string textWrappingStyle { get; set; }
        public string textWrappingType { get; set; }
        public double? verticalPosition { get; set; }
        public string verticalOrigin { get; set; }
        public string verticalAlignment { get; set; }
        public double? verticalRelativePercent { get; set; }
        public double? horizontalPosition { get; set; }
        public string horizontalOrigin { get; set; }
        public string horizontalAlignment { get; set; }
        public double? horizontalRelativePercent { get; set; }
        public int? zOrderPosition { get; set; }
        public bool? allowOverlap { get; set; }
        public bool? layoutInCell { get; set; }
        public bool? lockAnchor { get; set; }
        public double? distanceBottom { get; set; }
        public double? distanceLeft { get; set; }
        public double? distanceRight { get; set; }
        public double? distanceTop { get; set; }
        public string autoShapeType { get; set; }
        public TextFrame textFrame { get; set; }
        public string name { get; set; }
        public bool? hasFieldEnd { get; set; }
        public int? fieldType { get; set; }
    }

    public class Left
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Level
    {
        public int startAt { get; set; }
        public int restartLevel { get; set; }
        public string listLevelPattern { get; set; }
        public string followCharacter { get; set; }
        public string numberFormat { get; set; }
        public CharacterFormat characterFormat { get; set; }
        public ParagraphFormat paragraphFormat { get; set; }
    }

    public class LineFormat
    {
        public bool line { get; set; }
        public string color { get; set; }
        public double weight { get; set; }
        public string lineStyle { get; set; }
        public string lineFormatType { get; set; }
    }

    public class List
    {
        public int listId { get; set; }
        public int abstractListId { get; set; }
    }

    public class ListFormat
    {
        public int listLevelNumber { get; set; }
        public int listId { get; set; }
    }

    public class ParagraphFormat
    {
        public string styleName { get; set; }
        public double? beforeSpacing { get; set; }
        public double? lineSpacing { get; set; }
        public string lineSpacingType { get; set; }
        public string textAlignment { get; set; }
        public double? leftIndent { get; set; }
        public ListFormat listFormat { get; set; }
        public List<Tab> tabs { get; set; }
        public double? rightIndent { get; set; }
        public double? firstLineIndent { get; set; }
        public string outlineLevel { get; set; }
    }

    public class Right
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Root
    {
        public List<Section> sections { get; set; }
        public CharacterFormat characterFormat { get; set; }
        public List<List> lists { get; set; }
        public List<AbstractList> abstractLists { get; set; }
        public Background background { get; set; }
        public List<Style> styles { get; set; }
        public double defaultTabWidth { get; set; }
        public bool formatting { get; set; }
        public bool trackChanges { get; set; }
        public string protectionType { get; set; }
        public bool enforcement { get; set; }
        public bool dontUseHTMLParagraphAutoSpacing { get; set; }
        public bool alignTablesRowByRow { get; set; }
        public bool formFieldShading { get; set; }
        public Footnotes footnotes { get; set; }
        public Endnotes endnotes { get; set; }
    }

    public class Row
    {
        public RowFormat rowFormat { get; set; }
        public List<Cell> cells { get; set; }
    }

    public class RowFormat
    {
        public bool allowBreakAcrossPages { get; set; }
        public bool isHeader { get; set; }
        public double height { get; set; }
        public string heightType { get; set; }
        public Borders borders { get; set; }
        public double leftMargin { get; set; }
        public double rightMargin { get; set; }
        public double leftIndent { get; set; }
    }

    public class Section
    {
        public List<Block> blocks { get; set; }
        public HeadersFooters headersFooters { get; set; }
        public SectionFormat sectionFormat { get; set; }
    }

    public class SectionFormat
    {
        public double headerDistance { get; set; }
        public double footerDistance { get; set; }
        public double pageWidth { get; set; }
        public double pageHeight { get; set; }
        public double leftMargin { get; set; }
        public double rightMargin { get; set; }
        public double topMargin { get; set; }
        public double bottomMargin { get; set; }
        public bool differentFirstPage { get; set; }
        public bool differentOddAndEvenPages { get; set; }
        public bool bidi { get; set; }
        public bool restartPageNumbering { get; set; }
        public int pageStartingNumber { get; set; }
        public string endnoteNumberFormat { get; set; }
        public string footNoteNumberFormat { get; set; }
        public string restartIndexForFootnotes { get; set; }
        public string restartIndexForEndnotes { get; set; }
    }

    public class Separator
    {
        public CharacterFormat characterFormat { get; set; }
        public ParagraphFormat paragraphFormat { get; set; }
        public List<Inline> inlines { get; set; }
    }

    public class Shading
    {
        public string texture { get; set; }
        public string backgroundColor { get; set; }
    }

    public class Style
    {
        public string type { get; set; }
        public string name { get; set; }
        public string next { get; set; }
        public CharacterFormat characterFormat { get; set; }
        public string basedOn { get; set; }
        public ParagraphFormat paragraphFormat { get; set; }
        public string link { get; set; }
    }

    public class Tab
    {
        public string tabJustification { get; set; }
        public double position { get; set; }
        public string tabLeader { get; set; }
        public double deletePosition { get; set; }
    }

    public class TableFormat
    {
        public bool allowAutoFit { get; set; }
        public double leftMargin { get; set; }
        public double rightMargin { get; set; }
        public double leftIndent { get; set; }
        public string tableAlignment { get; set; }
        public string preferredWidthType { get; set; }
        public Borders borders { get; set; }
        public bool bidi { get; set; }
        public string horizontalPositionAbs { get; set; }
        public double horizontalPosition { get; set; }
    }

    public class TextFrame
    {
        public string textVerticalAlignment { get; set; }
        public double leftMargin { get; set; }
        public double rightMargin { get; set; }
        public double topMargin { get; set; }
        public double bottomMargin { get; set; }
        public List<Block> blocks { get; set; }
    }

    public class Top
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Vertical
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }
}
