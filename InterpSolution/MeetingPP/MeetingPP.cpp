// MeetingPP.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"
#include "LAlgebra.h"


using namespace std;

int main()
{
	//GrammyCluster * cl= new GrammyCluster[7];
	Vec3 v1(1, 2, 3);
	Vec3 v2 = v1*2;
	Vec3 v3 = v1 - v2;
	auto l = dist(v1, v1);
	Vec3 b(1, 0, 0);
	Vec3 c(-1, 1, 0);
	auto a = angle_gr(b, c);
	b = c;
	size_t n = 140000 * 66;
	//FILE *pFile;
	//pFile = fopen("C:\\Users\\User\\Source\\Repos\\Interpolator\\InterpSolution\\MeetingPP\\mdata.csv", "r");
	
	//auto arr = new TFloat[n];
	//for (size_t i = 0; i < n; i++)
	//{
	//	fscanf(pFile, SCANF_STR, &arr[i]);
	//	//cout << f << endl;

	//}
	//fclose(pFile);
	//for (size_t i = 0; i < 77; i++)
	//{

	//	cout << arr[i] << endl;

	//}
	//cout << "done "<<sizeof(arr)*n<<endl;
	//FILE *pFile2;
	//pFile2 = fopen("C:\\Users\\User\\Source\\Repos\\Interpolator\\InterpSolution\\MeetingPP\\mdata.bin", "wb");
	//fwrite(arr, sizeof(arr), n, pFile2);
	//delete[] arr;
	//fclose(pFile2);
	//cout << "done2" << endl;
	FILE *pFile3;
	pFile3 = fopen("C:\\Users\\User\\Source\\Repos\\Interpolator\\InterpSolution\\MeetingPP\\mdata.bin", "rb");
	auto arr2 = new TFloat[n];
	fread(arr2, sizeof(arr2), n, pFile3);
	fclose(pFile3);
	cout << "done3" << endl;
	for (size_t i = 0; i < 77; i++)
	{
		
		cout << arr2[i] << endl;

	}
	string s;
	cin >> s;
	cout << s;
	cin >> s;
    return 0;
}

