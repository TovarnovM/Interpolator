// MeetingPP.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"
#include "LAlgebra.h"


using namespace std;


int main()
{
	GrammyCluster cl;
	cout <<endl<< cl.n_elems << endl << endl;
	//cout << endl << cl.load_from_csv("C:\\mdata.csv") << endl << endl;
	cout << endl << cl.load_from_bin("C:\\mdata.bin") << endl << endl;
	
	size_t *counts;
	size_t count;
	Vec3** traects;

	bool good = cl.GetExtrimeTraects(Vec3(0, 300, 0), Vec3(1, 0, 0), Vec3(6000, 0, 10), 30, &traects, count, &counts);
	if (good) {
		for (size_t i = 0; i < count; i++)
		{
			cout << "Traect "<<i<<"; count = "<<counts[i]<<endl;
			for (size_t j = 0; j < counts[i]; j++)
			{
				cout << "(" << traects[i][j].x << "; " << traects[i][j].y << "; " << traects[i][j].z << ")" << endl;
			}
			cout << endl;
		}
	}

	//auto traect = cl.TraectToPoint(Vec3(0, 300, 0), Vec3(1, 0, 0), Vec3(6000, 0, 10), 30, count);
	//for (size_t i = 0; i < count; i++)
	//{
	//	cout << "(" << traect[i].x << "; " << traect[i].y << "; " << traect[i].z << ")" << endl;
	//}
	string s;
	cin >> s;
    return 0;
}

