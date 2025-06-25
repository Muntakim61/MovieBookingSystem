using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieBookingSystem.Services
{
    public class BookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Try to create a new booking if there is capacity.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="movieId"></param>
        /// <param name="hallId"></param>
        /// <param name="seatCount"></param>
        /// <returns>Created booking if successful, otherwise null</returns>
        public async Task<(bool Success, string Message, Booking? Booking)> CreateBookingAsync(string userId, int movieId, int hallId, int seatCount)
        {
            // Validate hall exists
            var hall = await _context.Halls.FindAsync(hallId);
            if (hall == null)
                return (false, "Selected hall does not exist.", null);

            // Calculate seats already booked and confirmed for this hall and movie
            var bookedSeatsCount = await _context.Bookings
                .Where(b => b.HallId == hallId
                            && b.MovieId == movieId
                            && b.Status == BookingStatus.Confirmed)
                .SumAsync(b => (int?)b.SeatCount) ?? 0;

            // Check if requested seats fit capacity
            if (bookedSeatsCount + seatCount > hall.Capacity)
                return (false, "Not enough available seats in the selected hall.", null);

            // Create booking
            var booking = new Booking
            {
                UserId = userId,
                MovieId = movieId,
                HallId = hallId,
                SeatCount = seatCount,
                BookingDate = DateTime.Now,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return (true, "Booking created and pending approval.", booking);
        }

        /// <summary>
        /// Approve a booking: can only approve Pending bookings and ensure capacity again before confirming.
        /// </summary>
        public async Task<(bool Success, string Message)> ApproveBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != BookingStatus.Pending)
                return (false, "Only pending bookings can be approved.");

            // Check capacity again before approving
            var hall = await _context.Halls.FindAsync(booking.HallId);
            if (hall == null)
                return (false, "Associated hall not found.");

            var confirmedSeatsCount = await _context.Bookings
                .Where(b => b.HallId == booking.HallId
                            && b.MovieId == booking.MovieId
                            && b.Status == BookingStatus.Confirmed)
                .SumAsync(b => (int?)b.SeatCount) ?? 0;

            if (confirmedSeatsCount + booking.SeatCount > hall.Capacity)
                return (false, "Cannot approve booking — hall capacity exceeded.");

            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            return (true, "Booking approved successfully.");
        }

        /// <summary>
        /// Cancel or reject a booking: only pending or confirmed bookings can be cancelled.
        /// </summary>
        public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                return (false, "Booking is already cancelled.");

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return (true, "Booking cancelled successfully.");
        }
    }
}
