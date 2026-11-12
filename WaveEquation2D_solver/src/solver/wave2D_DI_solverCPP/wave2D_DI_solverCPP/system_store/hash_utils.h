#pragma once

#include <bit>          // std::bit_cast (C++20)
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <type_traits>

namespace hash_utils 
{

    // -----------------------------------------------------------------------------
    // splitmix64 — finalizer with good avalanche
    // -----------------------------------------------------------------------------
    inline constexpr std::uint64_t splitmix64(std::uint64_t x) noexcept
    {
        x += 0x9e3779b97f4a7c15ull;
        x = (x ^ (x >> 30)) * 0xbf58476d1ce4e5b9ull;
        x = (x ^ (x >> 27)) * 0x94d049bb133111ebull;
        return x ^ (x >> 31);
    }

    // -----------------------------------------------------------------------------
    // fnv_mix — hash any trivially-copyable value into a running hash
    // -----------------------------------------------------------------------------
    template <class T>
        requires std::is_trivially_copyable_v<T>
    inline std::uint64_t fnv_mix(std::uint64_t h, const T& value) noexcept
    {
        std::uint64_t x = 0;
        static_assert(sizeof(T) <= sizeof(std::uint64_t),
            "fnv_mix only supports types up to 64 bits; "
            "hash larger structs field-by-field");
        std::memcpy(&x, &value, sizeof(T));
        return splitmix64(h ^ splitmix64(x + 0x9e3779b97f4a7c15ull));
    }

} // namespace hash_utils



