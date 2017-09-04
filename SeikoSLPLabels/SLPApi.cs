using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CACCCheckIn.Printing.NameBadgePrinter
{
    public class SLP
    {
        public const int FALSE = 0;
        public const int TRUE = 1;

        //
        // Constant definitions
        //
        public const int Attr_Normal = 0;
        public const int Attr_Bold = 1;
        public const int Attr_Italic = 2;
        public const int Attr_Underline = 4;

        public const int Text_Left = 0;
        public const int Text_Center = 1;
        public const int Text_Right = 2;

        public const int Orient_Landscape = 0;
        public const int Orient_Portrait = 1;

        public const int DEBUG_OFF = 0; // No debug output
        public const int DEBUG_ON = 1; // Backward compatible output (DEBUG_LOG + DEBUG_VIRTUAL_PRINT + DEBUG_OUT_DBG_STR)
        public const int DEBUG_LOG = 2; // Write debug messages to debug log file
        public const int DEBUG_VIRTUAL_PRINT = 4; // Output label image to clipboard, instead of printer
        public const int DEBUG_MSG_BOX = 8; // Enables debug message boxes (with OK button)
        public const int DEBUG_OUT_DBG_STR = 16; // Write debug messages to debugger ("OutputDebugString")
        public const int DEBUG_ENTER_EXIT = 32; // Log entry into and exit from API functions

        //
        // Label sizes
        //
        /// <summary>
        /// Standard Address
        /// 1.1" x 3.5"
        /// </summary>
        public const int Size_StandardAddress = 1;
        /// <summary>
        /// Shipping
        /// 2.15" x 4.0"
        /// </summary>
        public const int Size_Shipping = 2;
        /// <summary>
        /// Diskette
        /// 2.15" x 2.75"
        /// </summary>
        public const int Size_Diskette = 3;
        /// <summary>
        /// Large Address
        /// 1.4" x 3.5"
        /// </summary>
        public const int Size_LargeAddress = 4;
        /// <summary>
        /// Multi-Purpose
        /// 1.5" x 2.0"
        /// </summary>
        public const int Size_Multipurpose = 5;
        /// <summary>
        /// VHS Spine
        /// 0.75" x 5.8"
        /// </summary>
        public const int Size_VHS_Spine = 7;
        /// <summary>
        /// VHS Face
        /// 1.8" x 3.05"
        /// </summary>
        public const int Size_VHS_Face = 8;
        /// <summary>
        /// 8mm Spine
        /// 0.4" x 2.85"
        /// </summary>
        public const int Size_8mm_Spine = 9;
        /// <summary>
        /// File Folder
        /// 0.55" x 3.45"
        /// </summary>
        public const int Size_FileFolder = 10;
        /// <summary>
        /// 35mm Slide
        /// 0.45" x 1.5"
        /// </summary>
        public const int Size_35mm = 11;
        /// <summary>
        /// Name Badge
        /// 2.15" x 2.75"
        /// </summary>
        public const int Size_NameBadge = 12;
        /// <summary>
        /// Euro Folder (narrow)
        /// 1.5" x 7.45"
        /// </summary>
        public const int Size_Euro_Narrow = 13;
        /// <summary>
        /// Euro Folder (wide)
        /// 2.15" x 7.45"
        /// </summary>
        public const int Size_Euro_Wide = 14;
        /// <summary>
        /// Iomega® Zip™ Disk
        /// 1.95" x 2.3"
        /// </summary>
        public const int Size_ZipDisk = 15;
        /// <summary>
        /// Jewelry Tag
        /// 2.15" x 1.25"
        /// </summary>
        public const int Size_JewelryTag = 17;
        /// <summary>
        /// Euro Name Tag
        /// 1.50" x 2.65"
        /// </summary>
        public const int Size_EuroNameTag = 18;


        //
        // Bar Code Symbologies
        //
        public const int SLP_BC_CODE39 = 1;
        public const int SLP_BC_CODE2OF5 = 2;
        public const int SLP_BC_CODABAR = 3;
        public const int SLP_BC_CODE128 = 4;
        public const int SLP_BC_UPC = 5;
        public const int SLP_BC_UPCE = 6;
        public const int SLP_BC_EAN13 = 7;
        public const int SLP_BC_POSTNET = 8;
        public const int SLP_BC_RM4SCC = 9;
        public const int SLP_BC_MAXICODE = 20;
        public const int SLP_BC_PDF417 = 21;
        public const int SLP_BC_DATAMATRIX = 22;
        public const int SLP_BC_QR = 23;
    }

    public class SLP32
    {
        //
        // Declarations for SlpApi7x32.DLL subroutines and functions
        // 
        
        //SLPSDK_API BOOL	    __stdcall SlpOpenPrinter(LPSTR szPrinterName, int nID, BOOL fPortrait);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpOpenPrinter@12", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpOpenPrinter(string strPrinterName, int nID, int fPortrait);

        //SLPSDK_API HFONT    __stdcall SlpCreateFont(LPSTR wszName, int nPoints, int nAttributes);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpCreateFont@12", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpCreateFont(string lpName, int nPoints, int nAttributes);

        //SLPSDK_API int      __stdcall SlpGetErrorCode(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetErrorCode@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpGetErrorCode();

        //SLPSDK_API int    __stdcall SlpGetErrorString(LPSTR wszText, int nMaxLength);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetErrorString@8", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetErrorString(string lpText, int nMaxLength);
        
        //SLPSDK_API int      __stdcall SlpDeleteFont(HFONT hFont);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpDeleteFont@4", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpDeleteFont(int iFont);

        //SLPSDK_API int	    __stdcall SlpGetTextWidth(HFONT hFont, LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetTextWidth@8", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpGetTextWidth(int iFont, string lpText);

        //SLPSDK_API int	    __stdcall SlpGetTextHeight(HFONT hFont, LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetTextHeight@8", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpGetTextHeight(int iFont, string lpText);

        //SLPSDK_API int      __stdcall SlpGetLabelHeight(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "_SlpGetLabelHeight@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpGetLabelHeight();

        //SLPSDK_API int      __stdcall SlpGetLabelWidth(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetLabelWidth@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpGetLabelWidth();

        //SLPSDK_API BOOL	    __stdcall SlpStartLabel();
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpStartLabel@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpStartLabel();

        //SLPSDK_API BOOL     __stdcall SlpEndLabel();
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpEndLabel@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpEndLabel();

        //SLPSDK_API int      __stdcall SlpSetRotation(int nAngle);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpSetRotation@4", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpSetRotation(int nAngle);

        //SLPSDK_API BOOL	    __stdcall SlpDrawTextXY(int x, int y, HFONT hFont, LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpDrawTextXY@16", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpDrawTextXY(int x, int y, int iFont, string lpText);

        //SLPSDK_API void     __stdcall SlpClosePrinter(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpClosePrinter@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpClosePrinter();

        //SLPSDK_API BOOL     __stdcall SlpDrawRectangle(int x, int y, int width, int height, int thickness);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpDrawRectangle@20", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpDrawRectangle(int x, int y, int nWidth, int nHeight, int nThickness);

        //SLPSDK_API BOOL     __stdcall SlpDrawLine(int xStart, int yStart, int xEnd, int yEnd, int thickness);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpDrawLine@20", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpDrawLine(int xStart, int yStart, int xEnd, int yEnd, int nThickness);

        //SLPSDK_API BOOL     __stdcall SlpDrawPicture(int nLeft, int nTop, int nRight, int nBottom,
        //                                      LPSTR wszPath);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpDrawPicture@20", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpDrawPicture(int nLeft, int nTop, int nRight, int nBottom, string strPath);

        //SLPSDK_API BOOL     __stdcall SlpDrawBarCode(int nLeft, int nTop, int nRight, int nBottom,
        //                                      LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpDrawBarCode@20", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpDrawBarCode(int nLeft, int nTop, int nRight, int nBottom, string lpText);

        //SLPSDK_API BOOL     __stdcall SlpSetBarCodeStyle(int nSymbology, int nRatio, int nMode, 
        //                                          int nSecurity, BOOL bReadableText,
        //                                          int nFontHeight, int nFontAttributes, 
        //                                          LPSTR wszFaceName);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpSetBarCodeStyle@32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpSetBarCodeStyle(int nSymbology, int nRatio, int nMode, int nSecurity, int bReadableText, int nFontHeight, int nFontAttributes, string strFaceName);

        //SLPSDK_API int    __stdcall SlpGetBarCodeWidth(int nLeft, int nTop, int nRight, int nBottom, LPSTR lpText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetBarCodeWidth@20", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetBarCodeWidth(int nLeft, int nTop, int nRight, int nBottom, string lpText);

        //SLPSDK_API int    __stdcall SlpGetVersion(LPSTR wszVersion);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetVersion@4", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetVersion(string lpText);

        //SLPSDK_API int      __stdcall SlpFindPrinters(BOOL bAllPrinters);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpFindPrinters@4", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern int SlpFindPrinters(bool bAllPrinters);

        //SLPSDK_API int      __stdcall SlpGetPrinterDPI(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetPrinterDPI@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetPrinterDPI();

        //SLPSDK_API int    __stdcall SlpGetPrinterName(int nIndex, LPSTR wszPrinterName, int nMaxLength);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpGetPrinterName@12", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetPrinterName(int nIndex, string strPrinterName, int nMaxChars);

        //SLPSDK_API void     __stdcall SlpDebugMode(DWORD dwMode);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpDebugMode@4", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpDebugMode(int nMode);

        //SLPSDK_API void     __stdcall SlpCopyLabelToClipboard(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpCopyLabelToClipboard@0", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	    public static extern void SlpCopyLabelToClipboard();

        //SLPSDK_API void     __stdcall SlpComment(LPSTR wszComment);
        [System.Runtime.InteropServices.DllImport("SlpApi7x32", EntryPoint = "_SlpComment@4", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpComment(string wszComment);

        public static string GetErrorCodeDescription()
        {
            int errorCode = 0;
            string message = "";

            //// Error codes
            //#define API_NO_ERROR                             0
            //#define API_NO_DC                               -1
            //#define API_GENERIC_ERROR                       -2
            //#define API_BAD_LABEL_TYPE                      -3
            //#define API_BAD_FONT_HANDLE                     -4
            //#define API_BAD_THICKNESS                       -5
            //#define API_BAD_BAR_CODE_STYLE                  -6
            //#define API_BAD_PRINTER_TYPE                    -7
            //#define API_INVALID_PORT                        -8
            //#define API_INVALID_LABEL_TYPE                  -9
            //#define API_INVALID_BAR_CODE_HANDLE             -10
            //#define API_INVALID_BITMAP_HANDLE               -11
            //#define API_NO_BAR_CODE_LIB                     -12
            //#define API_INVALID_LIBRARY_FUNCTION            -13
            //#define API_PRINTER_OPEN_ERROR                  -14
            //#define API_INVALID_IMAGE_FILE                  -15
            //#define API_IMAGE_ERROR                         -16
            //#define API_BUFFER_TOO_SMALL                    -17
            //#define API_INVALID_ERROR                       -18

            errorCode = SLP32.SlpGetErrorCode();
            switch (errorCode)
            {
                case 0:
                    message = "No error";
                    break;
                case -1:
                    message = "No device context";
                    break;
                case -2:
                    message = "General error";
                    break;
                case -3:
                    message = "Invalid label type";
                    break;
                case -4:
                    message = "Invalid font handle";
                    break;
                case -5:
                    message = "Invalid thickness parameter";
                    break;
                case -6:
                    message = "Invalid bar code style";
                    break;
                case -7:
                    message = "Invalid printer model";
                    break;
                case -8:
                    message = "Invalid port";
                    break;
                case -9:
                    message = "Invalid label type";
                    break;
                case -10:
                    message = "Invalid bar code handle";
                    break;
                case -11:
                    message = "Invalid bitmap handle";
                    break;
                case -12:
                    message = "Bar code library is not loaded";
                    break;
                case -13:
                    message = "Invalid library function";
                    break;
                case -14:
                    message = "Printer open error";
                    break;
                case -15:
                    message = "Invalid image file";
                    break;
                case -16:
                    message = "Image processing error";
                    break;
                case -17:
                    message = "Buffer too small";
                    break;
                case -18:
                    message = "Invalid error";
                    break;
                default:
                    message = "Unexpected error " + SLP32.SlpGetErrorCode().ToString();
                    break;
            }

            return message;
        }
    }

    public class SLP64
    {
        //
        // Declarations for SlpApi7x64.DLL subroutines and functions
        // 

        //SLPSDK_API BOOL	    __stdcall SlpOpenPrinter(LPSTR szPrinterName, int nID, BOOL fPortrait);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpOpenPrinter", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpOpenPrinter(string strPrinterName, int nID, int fPortrait);

        //SLPSDK_API HFONT    __stdcall SlpCreateFont(LPSTR wszName, int nPoints, int nAttributes);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpCreateFont", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpCreateFont(string lpName, int nPoints, int nAttributes);

        //SLPSDK_API int      __stdcall SlpGetErrorCode(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetErrorCode", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetErrorCode();

        //SLPSDK_API int    __stdcall SlpGetErrorString(LPSTR wszText, int nMaxLength);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetErrorString", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetErrorString(string lpText, int nMaxLength);

        //SLPSDK_API int      __stdcall SlpDeleteFont(HFONT hFont);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpDeleteFont", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpDeleteFont(int iFont);

        //SLPSDK_API int	    __stdcall SlpGetTextWidth(HFONT hFont, LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetTextWidth", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetTextWidth(int iFont, string lpText);

        //SLPSDK_API int	    __stdcall SlpGetTextHeight(HFONT hFont, LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetTextHeight", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetTextHeight(int iFont, string lpText);

        //SLPSDK_API int      __stdcall SlpGetLabelHeight(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetLabelHeight", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetLabelHeight();

        //SLPSDK_API int      __stdcall SlpGetLabelWidth(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetLabelWidth", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetLabelWidth();

        //SLPSDK_API BOOL	    __stdcall SlpStartLabel();
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpStartLabel", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpStartLabel();

        //SLPSDK_API BOOL     __stdcall SlpEndLabel();
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpEndLabel", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpEndLabel();

        //SLPSDK_API int      __stdcall SlpSetRotation(int nAngle);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpSetRotation", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpSetRotation(int nAngle);

        //SLPSDK_API BOOL	    __stdcall SlpDrawTextXY(int x, int y, HFONT hFont, LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpDrawTextXY", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpDrawTextXY(int x, int y, int iFont, string lpText);

        //SLPSDK_API void     __stdcall SlpClosePrinter(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpClosePrinter", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpClosePrinter();

        //SLPSDK_API BOOL     __stdcall SlpDrawRectangle(int x, int y, int width, int height, int thickness);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpDrawRectangle", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpDrawRectangle(int x, int y, int nWidth, int nHeight, int nThickness);

        //SLPSDK_API BOOL     __stdcall SlpDrawLine(int xStart, int yStart, int xEnd, int yEnd, int thickness);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpDrawLine", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpDrawLine(int xStart, int yStart, int xEnd, int yEnd, int nThickness);

        //SLPSDK_API BOOL     __stdcall SlpDrawPicture(int nLeft, int nTop, int nRight, int nBottom,
        //                                      LPSTR wszPath);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpDrawPicture", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpDrawPicture(int nLeft, int nTop, int nRight, int nBottom, string strPath);

        //SLPSDK_API BOOL     __stdcall SlpDrawBarCode(int nLeft, int nTop, int nRight, int nBottom,
        //                                      LPSTR wszText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpDrawBarCode", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpDrawBarCode(int nLeft, int nTop, int nRight, int nBottom, string lpText);

        //SLPSDK_API BOOL     __stdcall SlpSetBarCodeStyle(int nSymbology, int nRatio, int nMode, 
        //                                          int nSecurity, BOOL bReadableText,
        //                                          int nFontHeight, int nFontAttributes, 
        //                                          LPSTR wszFaceName);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpSetBarCodeStyle", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpSetBarCodeStyle(int nSymbology, int nRatio, int nMode, int nSecurity, int bReadableText, int nFontHeight, int nFontAttributes, string strFaceName);

        //SLPSDK_API int    __stdcall SlpGetBarCodeWidth(int nLeft, int nTop, int nRight, int nBottom, LPSTR lpText);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetBarCodeWidth", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetBarCodeWidth(int nLeft, int nTop, int nRight, int nBottom, string lpText);

        //SLPSDK_API int    __stdcall SlpGetVersion(LPSTR wszVersion);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetVersion", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetVersion(string lpText);

        //SLPSDK_API int      __stdcall SlpFindPrinters(BOOL bAllPrinters);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpFindPrinters", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpFindPrinters(bool bAllPrinters);

        //SLPSDK_API int      __stdcall SlpGetPrinterDPI(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetPrinterDPI", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetPrinterDPI();

        //SLPSDK_API int    __stdcall SlpGetPrinterName(int nIndex, LPSTR wszPrinterName, int nMaxLength);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpGetPrinterName", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SlpGetPrinterName(int nIndex, string strPrinterName, int nMaxChars);

        //SLPSDK_API void     __stdcall SlpDebugMode(DWORD dwMode);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpDebugMode", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpDebugMode(int nMode);

        //SLPSDK_API void     __stdcall SlpCopyLabelToClipboard(void);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpCopyLabelToClipboard", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpCopyLabelToClipboard();

        //SLPSDK_API void     __stdcall SlpComment(LPSTR wszComment);
        [System.Runtime.InteropServices.DllImport("SlpApi7x64", EntryPoint = "SlpComment", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void SlpComment(string wszComment);

        public static string GetErrorCodeDescription()
        {
            int errorCode = 0;
            string message = "";

            //// Error codes
            //#define API_NO_ERROR                             0
            //#define API_NO_DC                               -1
            //#define API_GENERIC_ERROR                       -2
            //#define API_BAD_LABEL_TYPE                      -3
            //#define API_BAD_FONT_HANDLE                     -4
            //#define API_BAD_THICKNESS                       -5
            //#define API_BAD_BAR_CODE_STYLE                  -6
            //#define API_BAD_PRINTER_TYPE                    -7
            //#define API_INVALID_PORT                        -8
            //#define API_INVALID_LABEL_TYPE                  -9
            //#define API_INVALID_BAR_CODE_HANDLE             -10
            //#define API_INVALID_BITMAP_HANDLE               -11
            //#define API_NO_BAR_CODE_LIB                     -12
            //#define API_INVALID_LIBRARY_FUNCTION            -13
            //#define API_PRINTER_OPEN_ERROR                  -14
            //#define API_INVALID_IMAGE_FILE                  -15
            //#define API_IMAGE_ERROR                         -16
            //#define API_BUFFER_TOO_SMALL                    -17
            //#define API_INVALID_ERROR                       -18

            errorCode = SLP64.SlpGetErrorCode();
            switch (errorCode)
            {
                case 0:
                    message = "No error";
                    break;
                case -1:
                    message = "No device context";
                    break;
                case -2:
                    message = "General error";
                    break;
                case -3:
                    message = "Invalid label type";
                    break;
                case -4:
                    message = "Invalid font handle";
                    break;
                case -5:
                    message = "Invalid thickness parameter";
                    break;
                case -6:
                    message = "Invalid bar code style";
                    break;
                case -7:
                    message = "Invalid printer model";
                    break;
                case -8:
                    message = "Invalid port";
                    break;
                case -9:
                    message = "Invalid label type";
                    break;
                case -10:
                    message = "Invalid bar code handle";
                    break;
                case -11:
                    message = "Invalid bitmap handle";
                    break;
                case -12:
                    message = "Bar code library is not loaded";
                    break;
                case -13:
                    message = "Invalid library function";
                    break;
                case -14:
                    message = "Printer open error";
                    break;
                case -15:
                    message = "Invalid image file";
                    break;
                case -16:
                    message = "Image processing error";
                    break;
                case -17:
                    message = "Buffer too small";
                    break;
                case -18:
                    message = "Invalid error";
                    break;
                default:
                    message = "Unexpected error " + SLP64.SlpGetErrorCode().ToString();
                    break;
            }

            return message;
        }
    }
}
