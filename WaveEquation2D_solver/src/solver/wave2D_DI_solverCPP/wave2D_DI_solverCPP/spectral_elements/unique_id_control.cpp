#include "unique_id_control.h"

unique_id_control::unique_id_control(int start = 0) : next_id(start)
{

}


int unique_id_control::create_free_ids(std::vector<int> existing_ids)
{
    if (existing_ids.empty())
    {
        next_id = 0;
        return next_id;
    }

    std::sort(existing_ids.begin(), existing_ids.end());

    // Remove duplicates (Assuming there are no duplicates)
    // existing_ids.erase(std::unique(existing_ids.begin(), existing_ids.end()), existing_ids.end());

    int expected_id = 0; // start from 0 // existing_ids.front();  // start from first ID

    for (const int& id : existing_ids)
    {
        while (expected_id < id)
        {
            free_ids.push(expected_id);
            expected_id++;
        }
        expected_id = id + 1;
    }

    // Next new ID starts after largest existing ID
    next_id = expected_id;

    return next_id;
    //
}



int unique_id_control::get_unique_id()
{
    if (!free_ids.empty())
    {
		// Free ids are available, return the front of the queue
        int id = free_ids.front();
        free_ids.pop();
        return id;
    }
    else
    {
        return next_id++;
    }

    return -1;
}


void unique_id_control::release_id(int id)
{
    free_ids.push(id);
}


