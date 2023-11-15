#pragma once
#include <windows.h>
#include <string>
class app_2048
{
private:
	bool register_class(); //This function is used to register the window class for the application.

	static std::wstring const s_class_name;  // static member function that serves as the window procedure for the application.
	static LRESULT CALLBACK window_proc_static(HWND window, UINT message, WPARAM wparam, LPARAM lparam);

	LRESULT window_proc(HWND window, UINT message, WPARAM wparam, LPARAM lparam);//recieves messages and stores them
	//HWND create_window(DWORD style, HWND parent = nullptr); //This function creates the main window for the application and returns a handle to it.
	//create_window() was the older version when we had only 1 window.
	HWND create_window(DWORD style, HWND parent = nullptr, DWORD ex_style = 0); // extension for transparancy

	HINSTANCE m_instance;
	HWND window;
	HWND m_main;
	HWND m_popup;
	int cwidth = 15;
	int cheight = 60;
	int ball_x;
	int ball_y;
	int ball_r;
	int ball_speed_x;
	int ball_speed_y;
	HWND paddle;
	void update_transparency();
	void paddle_to_ball_collision();
	void movepaddle(int y);
	int p1_score;
	int p2_score;
	void resetgame();
	COLORREF bg= RGB(92, 243, 0);
	HWND balltrail;
	void DrawBallTrail(HDC hdc);
	HBITMAP bit;
	void LoadAndDisplayBitmap(HWND hwnd, LPCWSTR filename);
	WCHAR file[256];
public:

	app_2048(HINSTANCE instance);
	int run(int show_command);
};

