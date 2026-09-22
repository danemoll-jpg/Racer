#define UNICODE
#define _UNICODE
#define NOMINMAX
#include <windows.h>
#include <shellapi.h>
#include <gdiplus.h>
#include <xinput.h>
#include <filesystem>
#include <fstream>
#include <string>
#include <vector>
#include <chrono>
#include <algorithm>
#pragma comment(lib,"gdiplus.lib")
#pragma comment(lib,"shell32.lib")
#pragma comment(lib,"user32.lib")
#pragma comment(lib,"gdi32.lib")
namespace fs=std::filesystem;
static HWND window;
static std::wstring game,cover,evidence,save,status=L"Ready to play your installed game.";
static PROCESS_INFORMATION child{};
static bool testing=false,played=false;static int selected=0;static ULONGLONG launched=0;
static std::vector<std::wstring> labels={L"Play Woodstock Rush",L"Update controls (prototype)",L"Exit"};
static void Log(const std::string& line){if(!evidence.empty()){fs::create_directories(evidence);std::ofstream(fs::path(evidence)/L"lifetime.txt",std::ios::app)<<line<<"\n";}}
static std::wstring Quote(const std::wstring& s){std::wstring r=L"\"";size_t slash=0;for(wchar_t c:s){if(c==L'\\'){slash++;continue;}if(c==L'\"')r.append(slash*2+1,L'\\');else r.append(slash,L'\\');slash=0;r+=c;}r.append(slash*2,L'\\');return r+L"\"";}
static void Text(Gdiplus::Graphics& g,const std::wstring& s,float x,float y,float size,Gdiplus::Color color,float width=540){Gdiplus::Font font(L"Segoe UI",size,Gdiplus::FontStyleRegular,Gdiplus::UnitPixel);Gdiplus::SolidBrush brush(color);Gdiplus::RectF rect(x,y,width,80);g.DrawString(s.c_str(),-1,&font,rect,nullptr,&brush);}
static void Draw(Gdiplus::Graphics& g){
 g.Clear(Gdiplus::Color(255,12,23,27));g.SetSmoothingMode(Gdiplus::SmoothingModeAntiAlias);
 if(fs::exists(cover)){Gdiplus::Bitmap art(cover.c_str());if(art.GetLastStatus()==Gdiplus::Ok){float scale=std::min(425.f/art.GetWidth(),500.f/art.GetHeight());int w=int(art.GetWidth()*scale),h=int(art.GetHeight()*scale);g.DrawImage(&art,20+(425-w)/2,65+(500-h)/2,w,h);}}
 Text(g,L"WOODSTOCK RUSH",480,46,32,Gdiplus::Color(255,107,239,211));Text(g,L"Installed game • launcher prototype",480,96,17,Gdiplus::Color(255,182,195,198));
 for(int i=0;i<3;i++){Gdiplus::SolidBrush bg(i==selected?Gdiplus::Color(255,38,114,113):Gdiplus::Color(255,29,48,58));g.FillRectangle(&bg,480,175+i*74,550,58);Text(g,labels[i],500,float(185+i*74),22,Gdiplus::Color::White);}
 Text(g,status,480,422,18,Gdiplus::Color(255,215,223,220));Text(g,L"D-pad / arrows: choose   A / Enter: confirm\nB / Esc: exit",480,540,17,Gdiplus::Color(255,164,184,190));
}
static void Snapshot(){if(evidence.empty())return;Gdiplus::Bitmap bmp(1080,640,PixelFormat32bppARGB);Gdiplus::Graphics g(&bmp);Draw(g);UINT count=0,bytes=0;Gdiplus::GetImageEncodersSize(&count,&bytes);std::vector<BYTE> buf(bytes);auto enc=(Gdiplus::ImageCodecInfo*)buf.data();Gdiplus::GetImageEncoders(count,bytes,enc);for(UINT i=0;i<count;i++)if(wcscmp(enc[i].MimeType,L"image/png")==0){bmp.Save((fs::path(evidence)/L"prototype.png").c_str(),&enc[i].Clsid);break;}}
static BOOL CALLBACK CloseChild(HWND hwnd,LPARAM pid){DWORD found;GetWindowThreadProcessId(hwnd,&found);if(found==(DWORD)pid)PostMessageW(hwnd,WM_CLOSE,0,0);return TRUE;}
static void Play(){
 if(child.hProcess)return;if(!fs::exists(game)){status=L"Game not found. Your files have not been changed.";InvalidateRect(window,nullptr,FALSE);return;}
 std::wstring command=Quote(game);
 if(testing){fs::create_directories(save);std::ofstream(fs::path(save)/L"settings.json")<<R"({"version":1,"master":0,"frameLimit":60,"vsync":false})";command+=L" -racerSkipTitle -batchmode -racerTestSave "+Quote(save)+L" -logFile "+Quote((fs::path(evidence)/L"game.log").wstring());}
 STARTUPINFOW startup{sizeof(startup)};if(testing){startup.dwFlags=STARTF_USESHOWWINDOW;startup.wShowWindow=SW_HIDE;}
 if(!CreateProcessW(game.c_str(),command.data(),nullptr,nullptr,FALSE,0,nullptr,fs::path(game).parent_path().c_str(),&startup,&child)){status=L"The game could not start. Play can be retried.";Log("FAIL CreateProcess "+std::to_string(GetLastError()));return;}
 CloseHandle(child.hThread);child.hThread=nullptr;launched=GetTickCount64();played=true;Log("PASS Child launched with game directory as working directory; parent remains alive");status=L"Game running. Launcher keeps the Steam session alive.";if(!testing)ShowWindow(window,SW_HIDE);
}
static void Confirm(){if(selected==0)Play();else if(selected==2&&!child.hProcess)PostQuitMessage(0);else{status=L"Signed updates are the next implementation step.";InvalidateRect(window,nullptr,FALSE);}}
static void Key(UINT key){if(key==VK_UP)selected=(selected+2)%3;else if(key==VK_DOWN)selected=(selected+1)%3;else if(key==VK_RETURN||key==VK_SPACE)Confirm();else if(key==VK_ESCAPE&&!child.hProcess)PostQuitMessage(0);InvalidateRect(window,nullptr,FALSE);}
static LRESULT CALLBACK Proc(HWND h,UINT message,WPARAM w,LPARAM l){
 switch(message){
 case WM_PAINT:{PAINTSTRUCT p;HDC dc=BeginPaint(h,&p);Gdiplus::Graphics g(dc);Draw(g);EndPaint(h,&p);return 0;}
 case WM_KEYDOWN:Key((UINT)w);return 0;
 case WM_LBUTTONUP:{int y=HIWORD(l),x=LOWORD(l);if(x>=480&&x<=1030&&y>=175&&y<175+3*74){selected=(y-175)/74;Confirm();}return 0;}
 case WM_TIMER:{
   using GetState=DWORD(WINAPI*)(DWORD,XINPUT_STATE*);static HMODULE module=LoadLibraryW(L"xinput1_4.dll");static auto poll=module?(GetState)GetProcAddress(module,"XInputGetState"):nullptr;static WORD prior=0;
   if(poll&&!child.hProcess){XINPUT_STATE state{};if(poll(0,&state)==ERROR_SUCCESS){WORD edge=state.Gamepad.wButtons&~prior;prior=state.Gamepad.wButtons;if(edge&XINPUT_GAMEPAD_DPAD_UP)Key(VK_UP);if(edge&XINPUT_GAMEPAD_DPAD_DOWN)Key(VK_DOWN);if(edge&XINPUT_GAMEPAD_A)Key(VK_RETURN);if(edge&XINPUT_GAMEPAD_B)Key(VK_ESCAPE);}}
   if(child.hProcess){if(WaitForSingleObject(child.hProcess,0)==WAIT_OBJECT_0){DWORD code;GetExitCodeProcess(child.hProcess,&code);Log("Child exit="+std::to_string(code)+"; parent closes after child");CloseHandle(child.hProcess);child.hProcess=nullptr;PostQuitMessage(code?1:0);}else if(testing&&GetTickCount64()-launched>10000){Log("PASS Parent still alive while child runs; request normal WM_CLOSE");EnumWindows(CloseChild,child.dwProcessId);testing=false;}}
   return 0;}
 case WM_CLOSE:if(!child.hProcess)DestroyWindow(h);return 0;
 case WM_DESTROY:PostQuitMessage(0);return 0;
 }return DefWindowProcW(h,message,w,l);
}
int WINAPI wWinMain(HINSTANCE instance,HINSTANCE,PWSTR,int show){
 int argc;auto argv=CommandLineToArgvW(GetCommandLineW(),&argc);for(int i=1;i<argc;i++){std::wstring a=argv[i];if(a==L"--prototype-check")testing=true;else if(i+1<argc){if(a==L"--game")game=argv[++i];else if(a==L"--cover")cover=argv[++i];else if(a==L"--evidence")evidence=argv[++i];else if(a==L"--test-save")save=argv[++i];}}LocalFree(argv);
 Gdiplus::GdiplusStartupInput input;ULONG_PTR token;Gdiplus::GdiplusStartup(&token,&input,nullptr);WNDCLASSW wc{};wc.hInstance=instance;wc.lpfnWndProc=Proc;wc.lpszClassName=L"WoodstockRushLauncherPrototype";wc.hCursor=LoadCursor(nullptr,IDC_ARROW);RegisterClassW(&wc);
 window=CreateWindowW(wc.lpszClassName,L"Woodstock Rush",WS_OVERLAPPED|WS_CAPTION|WS_SYSMENU|WS_MINIMIZEBOX,CW_USEDEFAULT,CW_USEDEFAULT,1096,679,nullptr,nullptr,instance,nullptr);SetTimer(window,1,100,nullptr);
 if(testing){Log("Muted isolated-save prototype; physical controller/Deck not claimed");Key(VK_DOWN);Key(VK_UP);Log(selected==0?"PASS Keyboard/controller action selection model":"FAIL selection");Snapshot();Play();}else ShowWindow(window,show);
 MSG message;while(GetMessageW(&message,nullptr,0,0)>0){TranslateMessage(&message);DispatchMessageW(&message);}if(child.hProcess)CloseHandle(child.hProcess);Gdiplus::GdiplusShutdown(token);return (int)message.wParam;
}
