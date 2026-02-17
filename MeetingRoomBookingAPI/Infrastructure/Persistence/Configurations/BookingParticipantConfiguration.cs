using MeetingRoomBookingAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetingRoomBookingAPI.Infrastructure.Persistence.Configurations
{
    public class BookingParticipantConfiguration : IEntityTypeConfiguration<BookingParticipant>
    {
        public void Configure(EntityTypeBuilder<BookingParticipant> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => new { p.BookingId, p.UserId }).IsUnique();

            builder.HasOne(p => p.User)
                .WithMany(u => u.BookingParticipants)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
