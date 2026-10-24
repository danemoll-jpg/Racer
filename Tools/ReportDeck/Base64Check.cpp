// Steam Deck launcher round: the new strict decoder against Windows' own CryptStringToBinaryA (CRYPT_STRING_STRICT) and the real published signatures.
#include "../../Launcher/core.hpp"
#include <wincrypt.h>
#include <iostream>
#pragma comment(lib,"crypt32.lib")
using namespace wr;
static int Windows(const std::string& s,std::vector<BYTE>& out){DWORD n=0;if(!CryptStringToBinaryA(s.data(),(DWORD)s.size(),CRYPT_STRING_BASE64|CRYPT_STRING_STRICT,nullptr,&n,nullptr,nullptr))return 0;out.resize(n);if(!CryptStringToBinaryA(s.data(),(DWORD)s.size(),CRYPT_STRING_BASE64|CRYPT_STRING_STRICT,out.data(),&n,nullptr,nullptr))return 0;out.resize(n);return 1;}
static int New(const std::string& s,std::vector<BYTE>& out){try{out=Base64Decode(s);return 1;}catch(...){return 0;}}
int wmain(int argc,wchar_t** argv){json report=json::array();bool pass=true;
 auto test=[&](const std::string& name,const std::string& text,bool expect){std::vector<BYTE> a,b;int w=Windows(text,a),n=New(text,b);bool same=w==n&&(!w||a==b);bool ok=n==(int)expect&&(expect?same:true);pass&=ok;report.push_back({{"case",name},{"chars",text.size()},{"newAccepts",n==1},{"windowsAccepts",w==1},{"sameBytes",w&&n?a==b:false},{"expected",expect},{"pass",ok}});};
 for(int i=1;i<argc;i++){auto envelope=json::parse(Read(argv[i]));auto file=Utf8(fs::path(argv[i]).filename().wstring());test(file+" payload",envelope["payload"],true);test(file+" signature",envelope["signature"],true);json verified=Verify(Read(argv[i]));report.push_back({{"case",file+" Verify()"},{"type",verified.value("type","")},{"build",verified.value("build",0)},{"pass",true}});}
 test("one byte (2 pads)","YQ==",true);test("two bytes (1 pad)","YWI=",true);test("three bytes","YWJj",true);
 test("bad character $","YW$j",false);test("missing padding","YQ",false);test("missing one pad","YWI",false);test("pad in the middle","YQ==YWJj",false);test("three pads","Y===",false);test("space","YW Jj",false);test("line break","YWJj\r\nYWJj",false);test("url-safe alphabet","YW-_",false);test("empty","",false);
 std::string big(12*1024*1024,'A');test("oversized (12 MiB)",big,false);
 std::cout<<json{{"pass",pass},{"cases",report}}.dump(2)<<"\n";return pass?0:1;}
