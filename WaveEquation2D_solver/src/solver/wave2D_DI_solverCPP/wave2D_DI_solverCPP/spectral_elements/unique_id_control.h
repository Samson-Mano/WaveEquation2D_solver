#pragma once
#include <algorithm>
#include <vector>
#include <queue>

class unique_id_control
{
public:	
	unique_id_control(int start);
	~unique_id_control() = default;

	int create_free_ids(std::vector<int> existing_ids);
	int get_unique_id();
	void release_id(int id);

private:
	int next_id;
	std::queue<int> free_ids;

};
