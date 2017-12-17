#include "Funcs.h"

TFloat * get_dems_dx()
{
	TFloat * res = new TFloat[6]{
		25,10,25,3,3,17
	};
	return res;
}

size_t * get_dems_length()
{
	size_t * res = new size_t[7]{
		5,5,11,7,7,11,66
	};
	return res;
}

TFloat ** get_dems()
{
	TFloat** res = new TFloat*[6]{
		new TFloat[5]{ -50,-25,0,25,50 },
		new TFloat[5]{ 5,15,25,35,45 },
		new TFloat[11]{ 50,75,100,125,150,175,200,225,250,275,300 },
		new TFloat[7]{ -9,-6,-3,0,3,6,9 },
		new TFloat[7]{ -9,-6,-3,0,3,6,9 },
		new TFloat[11]{ -85,-68,-51,-34,-17,0,17,34,51,68,85 }
	};
	return res;
}





