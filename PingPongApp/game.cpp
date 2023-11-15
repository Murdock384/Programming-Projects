#include "game.h"
#include <stdexcept>
#include <Windows.h>
#include <Windowsx.h>
#include "resource.h"
#include <tchar.h>
#include <shlobj.h>
std::wstring const app_2048::s_class_name{ L"2048 Window" };


app_2048::app_2048(HINSTANCE instance) : m_instance{ instance }
{
    register_class();
    DWORD main_style = WS_OVERLAPPED | WS_SYSMENU | WS_CAPTION | WS_MINIMIZEBOX;
    DWORD popup_style = WS_OVERLAPPED | WS_CAPTION;
    m_main = create_window(main_style);
    m_popup = create_window(popup_style, m_main);
    SetLayeredWindowAttributes(m_popup, 0, 255, LWA_ALPHA);
    ball_x = 10;
    ball_y = 10;
    ball_r = 7;
    ball_speed_x = 5;
    ball_speed_y = 5;
}


bool app_2048::register_class()       //from WINAPI tutorial
{
    WNDCLASSEXW desc{};
    if (GetClassInfoExW(m_instance, s_class_name.c_str(),
        &desc) != 0)
        return true;

    desc = {
    .cbSize = sizeof(WNDCLASSEXW),
    .lpfnWndProc = window_proc_static,
    .hInstance = m_instance,
    .hIcon = LoadIcon(m_instance, MAKEINTRESOURCE(IDI_TUTORIAL)),
    .hCursor = LoadCursorW(nullptr, L"IDC_ARROW"),
    .hbrBackground = CreateSolidBrush(RGB(92,243,0)),//changes background
    .lpszMenuName = MAKEINTRESOURCEW(IDC_TUTORIAL),
    .lpszClassName = s_class_name.c_str(),
    .hIconSm= LoadIcon(m_instance, MAKEINTRESOURCE(IDI_SMALL))
    };
    return RegisterClassExW(&desc) != 0;
}


HWND app_2048::create_window(DWORD style, HWND parent, DWORD ex_style)
{

    RECT size;
    GetWindowRect(parent, &size); //getting dimensions of the parent window and putting it into size :https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect
   
    int x = (GetSystemMetrics(SM_CXSCREEN) - (size.right - size.left)) / 2; //calculating centre 

    int y = (GetSystemMetrics(SM_CYSCREEN) - (size.bottom - size.top)) / 2; 
    SetWindowPos(parent, NULL, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
    update_transparency();
    register_class();
    window = CreateWindowExW(
        ex_style,
        s_class_name.c_str(),
        L"Pong",
        style,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        500,
        350,
        parent,
        nullptr,
        m_instance,
        this);

    RECT child;
    GetClientRect(window, &child); //to get dimensions of the client area of the window
    int a = (child.right - cwidth);
    int b = (child.bottom - cheight) / 2;

    paddle = CreateWindowExW(
        0,
        L"STATIC",
        nullptr,
        WS_CHILD | WS_VISIBLE,
        a,
        b,
        cwidth,
        cheight,
        window,
        nullptr,
        m_instance,
        nullptr);


    balltrail= CreateWindowExW(
        WS_EX_LAYERED | WS_EX_TRANSPARENT, 
        L"STATIC",
        nullptr,
        WS_CHILD | WS_VISIBLE, 
        0, 0, 0, 0, 
        window, 
        nullptr, 
        m_instance,
        nullptr);
    SetLayeredWindowAttributes(balltrail, RGB(255, 255, 255), 128, LWA_COLORKEY | LWA_ALPHA);
 
    return window;
}


LRESULT app_2048::window_proc_static(
    HWND window,
    UINT message,
    WPARAM wparam,
    LPARAM lparam)
{
    app_2048* app = nullptr;
    if (message == WM_NCCREATE)
    {
        app = static_cast<app_2048*>(
            reinterpret_cast<LPCREATESTRUCTW>(lparam)->lpCreateParams);
        SetWindowLongPtrW(window, GWLP_USERDATA,
            reinterpret_cast<LONG_PTR>(app));

    }
    else
        app = reinterpret_cast<app_2048*>(GetWindowLongPtrW(window, GWLP_USERDATA));
    LRESULT res = app ?
        app->window_proc(window, message, wparam, lparam) :
        DefWindowProcW(window, message, wparam, lparam);
    if (message == WM_NCDESTROY)
        SetWindowLongPtrW(window, GWLP_USERDATA, 0);   //WINAPI TUTORIAL
    return res;
}

INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)   //WINAPI TUTORIAL
{
    UNREFERENCED_PARAMETER(lParam);
    switch (message)
    {
    case WM_INITDIALOG:
        return static_cast <INT_PTR>(TRUE);

    case WM_COMMAND:
        if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
        {
            EndDialog(hDlg, LOWORD(wParam));
            return static_cast <INT_PTR>(TRUE);
        }
        break;
    }
    return static_cast <INT_PTR>(FALSE);
}

LRESULT app_2048::window_proc(
    HWND window,
    UINT message,
    WPARAM wparam,
    LPARAM lparam)
{
    static int pad_y = 0;

    switch (message)
    {
    case WM_COMMAND:
    {
        int wmId = LOWORD(wparam);
        switch (wmId)
        {
        case IDM_ABOUT:
            DialogBox(m_instance, MAKEINTRESOURCE(IDD_ABOUTBOX), window, About);
            break;
        
        case IDM_EXIT:
            DestroyWindow(window);
            break;
        case ID_RESET:
           resetgame();
           break;
        case ID_BACKGROUND_COLOR:   //https://cpp.hotexamples.com/examples/-/-/ChooseColor/cpp-choosecolor-function-examples.html
            CHOOSECOLOR cc;
            static COLORREF acrCustClr[16];// array of custom colors
            static DWORD rgbCurrent;// initial color selection

            
            cc.lStructSize = sizeof(cc);

            cc.hwndOwner = nullptr;
            cc.lpCustColors = (LPDWORD)acrCustClr;
            cc.rgbResult = rgbCurrent;
            cc.Flags = CC_FULLOPEN | CC_RGBINIT;
            if (ChooseColor(&cc))
            {
                // Set the new background color
                HBRUSH hBrush = CreateSolidBrush(cc.rgbResult);
                bg = cc.rgbResult;
                SetClassLongPtr(window, (-10), (LONG_PTR)hBrush);
                InvalidateRect(window, nullptr, TRUE);
            }
            break;

        case ID_BACKGROUND_BITMAP:   //https://stackoverflow.com/questions/16791953/openfilename-open-dialog
        {
           
            OPENFILENAME ofn;
            
            ZeroMemory(&ofn, sizeof(ofn));
            ofn.lStructSize = sizeof(OPENFILENAME);
            ofn.hwndOwner = window;
            ofn.lpstrFile = file;
            ofn.nMaxFile = MAX_PATH;
            ofn.nFilterIndex = 1;
            ofn.Flags = OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_HIDEREADONLY;

            if (GetOpenFileName(&ofn) == TRUE)
            {
                //from tutorial
                bit = (HBITMAP)LoadImage(NULL, ofn.lpstrFile, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
                if (bit != NULL)
                {
                    HDC hdc = GetDC(window);
                    HDC hdcMem = CreateCompatibleDC(hdc);
                    SelectObject(hdcMem, bit);
                    BITMAP bm;
                    GetObject(bit, sizeof(bm), &bm);
                    StretchBlt(hdc, 0, 0, bm.bmWidth, bm.bmHeight, hdcMem, 0, 0, bm.bmWidth, bm.bmHeight, SRCCOPY);
                    DeleteDC(hdcMem);
                    ReleaseDC(window, hdc);
                    DeleteObject(bit);
                    
                }
                else
                {
                    MessageBox(window, L"Could'nt load bitmap", L"Error", MB_OK | MB_ICONERROR);
                }
            }
        }
       
        default:
            return DefWindowProc(window, message, wparam, lparam);
        }
    }
    break;
    case WM_PAINT:
    {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(window, &ps);

        // draw ball
        HBRUSH hBrush = CreateSolidBrush(RGB(255, 0, 0));  // red brush
        HBRUSH hOldBrush = (HBRUSH)SelectObject(hdc, hBrush);
        Ellipse(hdc, ball_x - ball_r, ball_y - ball_r, ball_x + ball_r, ball_y + ball_r);
        SelectObject(hdc, hOldBrush);
        DeleteObject(hBrush);
        DrawBallTrail(hdc);
        
        //from winapi tutorial PREV YEAR
        COLORREF inverse = RGB(255 - GetRValue(bg), 255 - GetGValue(bg), 255 - GetBValue(bg)); //http://www.vb-helper.com/howto_invert_color.html
        HFONT hFont = CreateFont(70, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE, DEFAULT_CHARSET, OUT_OUTLINE_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, DEFAULT_PITCH | FF_SWISS, _T("Arial"));
        HFONT hOldFont = (HFONT)SelectObject(hdc, hFont);

        SetBkMode(hdc, TRANSPARENT);
        SetTextColor(hdc, inverse);
        SetBkColor(hdc, RGB(0,0,0));

    
        TCHAR s[20];
        _stprintf_s(s, _T("%d"), p1_score);
        RECT rc;
        GetClientRect(window, &rc);
        rc.left = 3*(rc.right -rc.left)/4;
        rc.top = (rc.top - rc.bottom) / 1.7;
      
      
        DrawText(hdc, s, -1, &rc, DT_SINGLELINE | DT_LEFT | DT_VCENTER);

  
        SelectObject(hdc, hOldFont);
        DeleteObject(hFont);

        //Player 2 Score
        HFONT hFont1 = CreateFont(70, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE, DEFAULT_CHARSET, OUT_OUTLINE_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, DEFAULT_PITCH | FF_SWISS, _T("Arial"));
        HFONT hOldFont1 = (HFONT)SelectObject(hdc, hFont1);


        SetTextColor(hdc,inverse);
        SetBkMode(hdc, TRANSPARENT);
        SetBkColor(hdc, RGB(0, 0, 0));

        TCHAR s2[20];
        _stprintf_s(s2, _T("%d"), p2_score);
        RECT rc2;
        GetClientRect(window, &rc2);
        rc2.left = (rc2.right - rc2.left) / 5;
        rc2.top = (rc2.top - rc2.bottom) / 1.7;



        DrawText(hdc, s2, -1, &rc2, DT_SINGLELINE | DT_LEFT | DT_VCENTER);

        // select the old font back into the device context
        SelectObject(hdc, hOldFont);

        // delete the font object
        DeleteObject(hFont);
        LoadAndDisplayBitmap(window,file);

        EndPaint(window,&ps);
        break;
  
    }
    break;

    case WM_TIMER:
    {

        // update ball position
        ball_x += ball_speed_x;
        ball_y += ball_speed_y;

        RECT client;
        GetClientRect(window, &client);
        paddle_to_ball_collision();
        if (ball_x - ball_r < client.left)
        {
            ball_speed_x *= -1;  // reverse the direction in x-axis
        }

        if (ball_y - ball_r < client.top || ball_y + ball_r > client.bottom)
        {
            ball_speed_y *= -1;  // reverse the direction in y-axis
        }

        if (ball_x + ball_r > client.right) {
            ball_speed_x *= -1;
            p2_score++;
        }



        InvalidateRect(window, NULL, TRUE);
    }
    break;
    case WM_MOUSEMOVE:
        pad_y = HIWORD(lparam) - (cheight / 2); //determines y coordinate of mouse cursor
        movepaddle(pad_y);
        break;

        break;
    case WM_CLOSE:
        DestroyWindow(m_main);
        return 0;
    case WM_DESTROY:
        if (window == m_main)
            PostQuitMessage(EXIT_SUCCESS);
        return 0;
    

    }
    return DefWindowProcW(window, message, wparam, lparam);
}


void app_2048::paddle_to_ball_collision()  //https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-intersectrect
{
    RECT ball_rect{ ball_x - ball_r,ball_y - ball_r,ball_x + ball_r,ball_y + ball_r };

    HWND pad = FindWindowExW(m_main, nullptr, L"STATIC", nullptr);
    RECT paddle_rect;
    GetWindowRect(pad, &paddle_rect);
    MapWindowPoints(nullptr, m_main, reinterpret_cast<POINT*>(&paddle_rect), 2);

    if (IntersectRect(&ball_rect, &ball_rect, &paddle_rect))
    {
        ball_speed_x = -abs(ball_speed_x);
        if (ball_speed_x < 0)
            ball_x = paddle_rect.left - ball_r;
        else
            ball_x = paddle_rect.right + ball_r;
        p1_score++;
    }
}

void app_2048::movepaddle(int y)
{
    RECT mrect;
    GetWindowRect(GetParent(FindWindowExW(m_main, nullptr, L"STATIC", nullptr)), &mrect);

    RECT prect;
    GetWindowRect(FindWindowExW(m_main, nullptr, L"STATIC", nullptr), &prect);
    MapWindowPoints(nullptr, m_main, reinterpret_cast<POINT*>(&prect), 2);

    RECT win;
    GetClientRect(window, &win);

    if (ball_x + ball_r > win.right)
    {
        return;
    }

    int windowHeight = mrect.bottom - mrect.top - (cheight / 2 + 10);
    int paddleHeight = prect.bottom - prect.top;
    int newY = max(0, min(y, windowHeight - paddleHeight));

    MoveWindow(FindWindowExW(m_main, nullptr, L"STATIC", nullptr), prect.left, newY, prect.right - prect.left,
        prect.bottom - prect.top, true);
}
void app_2048::update_transparency()   //https://www.codeproject.com/Questions/61633/creating-a-transparent-window
{
    SetWindowLong(m_main, GWL_EXSTYLE, GetWindowLong(m_main, GWL_EXSTYLE) | WS_EX_LAYERED);
    SetLayeredWindowAttributes(m_main, 0, 255 * 80 / 100, LWA_ALPHA);
}


int app_2048::run(int show_command)   //WINAPI tutorial
{
    ShowWindow(m_main, show_command);
    MSG msg{};
    BOOL result = TRUE;
    SetTimer(m_main, 1, 50, NULL);

    HACCEL hAccelTable = LoadAccelerators(m_instance,MAKEINTRESOURCE(IDC_TUTORIAL));

    while ((result = GetMessageW(&msg, nullptr, 0, 0)) != 0)
    {
        if (result == -1)
            return EXIT_FAILURE;
        TranslateMessage(&msg);
        DispatchMessageW(&msg);

        if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }
    return EXIT_SUCCESS;
}




void app_2048::resetgame() 
{
    p1_score = 0;
    p2_score = 0;

    RECT g;
    GetClientRect(window, &g);
    int width = g.right - g.left;
    int height = g.bottom-g.top;
    ball_x = width/4;
    ball_y = height/4;
    ball_speed_x = abs(ball_speed_x);
    ball_speed_y = abs(ball_speed_y);
    InvalidateRect(window, NULL, TRUE);
}


void app_2048::DrawBallTrail(HDC hdc)
{
    // set the ball's properties
    COLORREF ballColor = RGB(255, 0, 0); // red color
    HBRUSH ballBrush = CreateSolidBrush(ballColor);
    SelectObject(hdc, ballBrush);
    HPEN ballPen = CreatePen(PS_SOLID, 1, ballColor);
    HPEN oldPen2 = (HPEN)SelectObject(hdc, ballPen);

    int trailRadius = ball_r / 1.3;
    int trailOpacity = 50;
    int numTrails = 5; // number of circles in the trail
    int x = ball_x - (ball_speed_x * 2); // position the trail circles behind the ball
    int y = ball_y - (ball_speed_y *2);
    int spacing = trailRadius /20; // spacing between the circles
    int radius = trailRadius;
    for (int i = numTrails; i >= 1; i--)
    {

        int opacity = trailOpacity * i / numTrails;


        COLORREF trailColor = RGB(255, 0, 0);    // red color
        HBRUSH trailBrush = CreateSolidBrush(trailColor);


        HBRUSH oldBrush = (HBRUSH)SelectObject(hdc, trailBrush);
        Ellipse(hdc, x - radius, y - radius, x + radius, y + radius);


        SelectObject(hdc, oldBrush);
        DeleteObject(trailBrush);


        x -= ball_speed_x * 2;
        y -= ball_speed_y * 2;
        radius -= trailRadius / numTrails;
    }


    SelectObject(hdc, oldPen2);
    DeleteObject(ballBrush);
    DeleteObject(ballPen);
}


void app_2048::LoadAndDisplayBitmap(HWND hwnd, LPCWSTR filename)    //https://learn.microsoft.com/en-us/windows/win32/gdiplus/-gdiplus-loading-and-displaying-bitmaps-use, https://stackoverflow.com/questions/57945118/c-win32-loading-multiple-bitmaps-in-wm-create-wont-load
{
    
    HBITMAP hBitmap = (HBITMAP)LoadImage(NULL, filename, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
    if (hBitmap == NULL)
    {
        return;
    }

    HDC hdc = GetDC(hwnd);

    HDC memDC = CreateCompatibleDC(hdc);
   
    HBITMAP oldBitmap = (HBITMAP)SelectObject(memDC, hBitmap);
 
    BITMAP bmp;
    GetObject(hBitmap, sizeof(BITMAP), &bmp);
 
    BitBlt(hdc, 0, 0, bmp.bmWidth, bmp.bmHeight, memDC, 0, 0, SRCCOPY);
   
    SelectObject(memDC, oldBitmap);
    DeleteDC(memDC);
    ReleaseDC(hwnd, hdc);
    DeleteObject(hBitmap);
}

/*
https://stackoverflow.com/questions/3463471/how-to-set-background-color-of-window-after-i
https://stackoverflow.com/questions/3970066/creating-a-transparent-window-in-c-win32
https://stackoverflow.com/questions/4631706/is-moving-a-window-with-setwindowpos-the-normal-way-to-do
https://stackoverflow.com/questions/33048092/calling-movewindow-with-brepaint-set-to-true
https://stackoverflow.com/questions/72505177/how-to-display-the-console-application-window-in-the-center-of-the-screen
*/