#include "stopwatch_events.h"

stopwatch_events::stopwatch_events()
{
	// Empty constructor
}

void stopwatch_events::start()
{
	// Start of the stop watch
	m_StartTime = std::chrono::high_resolution_clock::now();
	m_bRunning = true;

}

void stopwatch_events::stop()
{
	// Stop the watch
	m_EndTime = std::chrono::high_resolution_clock::now();
	m_bRunning = false;

}

double stopwatch_events::elapsed() const
{
	std::chrono::time_point<std::chrono::high_resolution_clock> endTime;

	if (m_bRunning == true)
	{
		// Stop watch is running (so no end time)
		endTime = std::chrono::high_resolution_clock::now();
	}
	else
	{
		// Stop watch is stopped
		endTime = m_EndTime;
	}

	// Returns the value in seconds
	return std::chrono::duration_cast<std::chrono::milliseconds>(endTime - m_StartTime).count() / 1000.0;

}



