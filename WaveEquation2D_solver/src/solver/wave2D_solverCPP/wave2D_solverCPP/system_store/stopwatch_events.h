#pragma once
#include <chrono>

class stopwatch_events
{
public:
    stopwatch_events();
    ~stopwatch_events() = default;
    void start();
    void stop();
    double elapsed() const;


private:
    std::chrono::time_point<std::chrono::high_resolution_clock> m_StartTime;
    std::chrono::time_point<std::chrono::high_resolution_clock> m_EndTime;
    bool m_bRunning = false;
};


