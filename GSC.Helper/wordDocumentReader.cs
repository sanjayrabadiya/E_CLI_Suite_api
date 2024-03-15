using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Helper
{

    public class SectionsHeader
    {
        public int DocumentId { get; set; }
        public int DocumentReviewId { get; set; }
        public int SeqNo { get; set; }
        public string DocumentName { get; set; }
        public int SectionNo { get; set; }
        public string SectionName { get; set; }
        public string Header { get; set; }
        public bool IsReadCompelete { get; set; }
        public bool IsReviewed { get; set; }
        public int ReviewTime { get; set; }
    }
    public class Tab
    {
        public string tabJustification { get; set; }
        public double position { get; set; }
        public string tabLeader { get; set; }
        public double deletePosition { get; set; }
    }

    public class ListFormat
    {
        public int listLevelNumber { get; set; }
        public int listId { get; set; }
    }

    public class ParagraphFormat
    {
        public string styleName { get; set; }
        public List<Tab> tabs { get; set; }
        public ListFormat listFormat { get; set; }
    }

    public class CharacterFormat
    {
        public string fontColor { get; set; }
        public bool? bold { get; set; }
    }

    public class ChartData
    {
        public double yValue { get; set; }
    }

    public class ChartCategory
    {
        public List<ChartData> chartData { get; set; }
        public string categoryXName { get; set; }
    }

    public class Fill
    {
        public string foreColor { get; set; }
        public string rgb { get; set; }
    }

    public class Line
    {
        public string color { get; set; }
        public string rgb { get; set; }
    }

    public class DataPoint
    {
        public Fill fill { get; set; }
        public Line line { get; set; }
    }

    public class ChartSery
    {
        public string seriesName { get; set; }
        public List<DataPoint> dataPoints { get; set; }
    }

    public class ChartArea
    {
        public string foreColor { get; set; }
    }

    public class Layout
    {
        public double layoutX { get; set; }
        public double layoutY { get; set; }
    }

    public class Fill2
    {
        public string foreColor { get; set; }
        public string rgb { get; set; }
    }

    public class Line2
    {
        public string color { get; set; }
        public string rgb { get; set; }
    }

    public class DataFormat
    {
        public Fill2 fill { get; set; }
        public Line2 line { get; set; }
    }

    public class ChartTitleArea
    {
        public Layout layout { get; set; }
        public string fontName { get; set; }
        public double fontSize { get; set; }
        public DataFormat dataFormat { get; set; }
    }

    public class PlotArea
    {
        public string foreColor { get; set; }
    }

    public class Layout2
    {
        public double layoutX { get; set; }
        public double layoutY { get; set; }
    }

    public class Fill3
    {
        public string foreColor { get; set; }
        public string rgb { get; set; }
    }

    public class Line3
    {
        public string color { get; set; }
        public string rgb { get; set; }
    }

    public class DataFormat2
    {
        public Fill3 fill { get; set; }
        public Line3 line { get; set; }
    }

    public class ChartTitleArea2
    {
        public Layout2 layout { get; set; }
        public string fontName { get; set; }
        public double fontSize { get; set; }
        public DataFormat2 dataFormat { get; set; }
    }

    public class ChartLegend
    {
        public string position { get; set; }
        public ChartTitleArea2 chartTitleArea { get; set; }
    }

    public class ChartPrimaryCategoryAxis
    {
        public object chartTitle { get; set; }
        public double fontSize { get; set; }
        public string fontName { get; set; }
        public string categoryType { get; set; }
        public string numberFormat { get; set; }
        public double minimumValue { get; set; }
        public double maximumValue { get; set; }
        public double majorUnit { get; set; }
        public bool hasMajorGridLines { get; set; }
        public bool hasMinorGridLines { get; set; }
        public string majorTickMark { get; set; }
        public string minorTickMark { get; set; }
        public string tickLabelPosition { get; set; }
    }

    public class ChartPrimaryValueAxis
    {
        public object chartTitle { get; set; }
        public double fontSize { get; set; }
        public string fontName { get; set; }
        public double minimumValue { get; set; }
        public double maximumValue { get; set; }
        public double majorUnit { get; set; }
        public bool hasMajorGridLines { get; set; }
        public bool hasMinorGridLines { get; set; }
        public string majorTickMark { get; set; }
        public string minorTickMark { get; set; }
        public string tickLabelPosition { get; set; }
    }

    public class Inline
    {
        public string text { get; set; }
        public CharacterFormat characterFormat { get; set; }
        public string imageString { get; set; }
        public int? length { get; set; }
        public double? width { get; set; }
        public double? height { get; set; }
        public bool? isInlineImage { get; set; }
        public bool? isMetaFile { get; set; }
        public double? top { get; set; }
        public double? bottom { get; set; }
        public double? right { get; set; }
        public double? left { get; set; }
        public double? getimageheight { get; set; }
        public double? getimagewidth { get; set; }
        public List<ChartCategory> chartCategory { get; set; }
        public List<ChartSery> chartSeries { get; set; }
        public ChartArea chartArea { get; set; }
        public ChartTitleArea chartTitleArea { get; set; }
        public PlotArea plotArea { get; set; }
        public ChartLegend chartLegend { get; set; }
        public ChartPrimaryCategoryAxis chartPrimaryCategoryAxis { get; set; }
        public ChartPrimaryValueAxis chartPrimaryValueAxis { get; set; }
        public object chartTitle { get; set; }
        public string chartType { get; set; }
        public double? gapWidth { get; set; }
        public double? overlap { get; set; }
        public object chartDataTable { get; set; }
    }

    public class CharacterFormat2
    {
        public string fontColor { get; set; }
        public bool? bold { get; set; }
    }

    public class Left
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Right
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Top
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Bottom
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Vertical
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Horizontal
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
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

    public class RowFormat
    {
        public bool allowBreakAcrossPages { get; set; }
        public bool isHeader { get; set; }
        public double height { get; set; }
        public string heightType { get; set; }
        public Borders borders { get; set; }
    }

    public class CharacterFormat3
    {
        public string fontColor { get; set; }
    }

    public class ParagraphFormat2
    {
        public string styleName { get; set; }
    }

    public class CharacterFormat4
    {
        public string fontColor { get; set; }
    }

    public class Inline2
    {
        public string text { get; set; }
        public CharacterFormat4 characterFormat { get; set; }
    }

    public class Block2
    {
        public CharacterFormat3 characterFormat { get; set; }
        public ParagraphFormat2 paragraphFormat { get; set; }
        public List<Inline2> inlines { get; set; }
    }

    public class Left2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Right2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Top2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Bottom2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Vertical2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Horizontal2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class DiagonalDown2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class DiagonalUp2
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Borders2
    {
        public Left2 left { get; set; }
        public Right2 right { get; set; }
        public Top2 top { get; set; }
        public Bottom2 bottom { get; set; }
        public Vertical2 vertical { get; set; }
        public Horizontal2 horizontal { get; set; }
        public DiagonalDown2 diagonalDown { get; set; }
        public DiagonalUp2 diagonalUp { get; set; }
    }

    public class CellFormat
    {
        public int columnSpan { get; set; }
        public int rowSpan { get; set; }
        public double preferredWidth { get; set; }
        public string preferredWidthType { get; set; }
        public string verticalAlignment { get; set; }
        public bool isSamePaddingAsTable { get; set; }
        public Borders2 borders { get; set; }
        public double cellWidth { get; set; }
    }

    public class Cell
    {
        public List<Block2> blocks { get; set; }
        public CellFormat cellFormat { get; set; }
    }

    public class Row
    {
        public RowFormat rowFormat { get; set; }
        public List<Cell> cells { get; set; }
    }

    public class Left3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Right3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Top3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Bottom3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Vertical3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class Horizontal3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
        public string color { get; set; }
    }

    public class DiagonalDown3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class DiagonalUp3
    {
        public string lineStyle { get; set; }
        public double lineWidth { get; set; }
        public bool shadow { get; set; }
        public double space { get; set; }
        public bool hasNoneStyle { get; set; }
    }

    public class Borders3
    {
        public Left3 left { get; set; }
        public Right3 right { get; set; }
        public Top3 top { get; set; }
        public Bottom3 bottom { get; set; }
        public Vertical3 vertical { get; set; }
        public Horizontal3 horizontal { get; set; }
        public DiagonalDown3 diagonalDown { get; set; }
        public DiagonalUp3 diagonalUp { get; set; }
    }

    public class TableFormat
    {
        public bool allowAutoFit { get; set; }
        public double leftIndent { get; set; }
        public string tableAlignment { get; set; }
        public string preferredWidthType { get; set; }
        public Borders3 borders { get; set; }
        public bool bidi { get; set; }
        public string horizontalPositionAbs { get; set; }
        public double horizontalPosition { get; set; }
    }

    public class Block
    {
        public ParagraphFormat paragraphFormat { get; set; }
        public List<Inline> inlines { get; set; }
        public CharacterFormat2 characterFormat { get; set; }
        public List<Row> rows { get; set; }
        public object title { get; set; }
        public object description { get; set; }
        public TableFormat tableFormat { get; set; }
    }

    public class HeadersFooters
    {
        //Empty class
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
    }

    public class Section
    {
        public List<Block> blocks { get; set; }
        public HeadersFooters headersFooters { get; set; }
        public SectionFormat sectionFormat { get; set; }
    }

    public class CharacterFormat5
    {
        public double fontSize { get; set; }
        public string fontFamily { get; set; }
        public string fontColor { get; set; }
        public double fontSizeBidi { get; set; }
        public string fontFamilyBidi { get; set; }
    }

    public class ParagraphFormat3
    {
        public double afterSpacing { get; set; }
        public double lineSpacing { get; set; }
        public string lineSpacingType { get; set; }
    }

    public class List
    {
        public int listId { get; set; }
        public int abstractListId { get; set; }
    }

    public class CharacterFormat6
    {
        public string fontFamily { get; set; }
        public string fontColor { get; set; }
        public string fontFamilyBidi { get; set; }
    }

    public class ParagraphFormat4
    {
        public double leftIndent { get; set; }
        public double firstLineIndent { get; set; }
    }

    public class Level
    {
        public string listLevelPattern { get; set; }
        public string followCharacter { get; set; }
        public string numberFormat { get; set; }
        public CharacterFormat6 characterFormat { get; set; }
        public ParagraphFormat4 paragraphFormat { get; set; }
    }

    public class AbstractList
    {
        public int abstractListId { get; set; }
        public List<Level> levels { get; set; }
    }

    public class Background
    {
        public string color { get; set; }
    }

    public class CharacterFormat7
    {
        public string fontColor { get; set; }
        public bool? bold { get; set; }
        public double? fontSize { get; set; }
        public string fontFamily { get; set; }
        public bool? boldBidi { get; set; }
        public double? fontSizeBidi { get; set; }
        public string fontFamilyBidi { get; set; }
    }

    public class ParagraphFormat5
    {
        public double beforeSpacing { get; set; }
        public double afterSpacing { get; set; }
        public string outlineLevel { get; set; }
        public double? lineSpacing { get; set; }
        public string lineSpacingType { get; set; }
        public double? leftIndent { get; set; }
        public bool? contextualSpacing { get; set; }
    }

    public class Style
    {
        public string type { get; set; }
        public string name { get; set; }
        public string next { get; set; }
        public CharacterFormat7 characterFormat { get; set; }
        public string basedOn { get; set; }
        public string link { get; set; }
        public ParagraphFormat5 paragraphFormat { get; set; }
    }

    public class Root
    {
        public List<Section> sections { get; set; }
        public CharacterFormat5 characterFormat { get; set; }
        public ParagraphFormat3 paragraphFormat { get; set; }
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
    }


}
