using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DomusMind.Application.Tests.EventLog
{
    public sealed class EventLogWriterTests
    {
        // Use test event from DomusMind.Family.Events namespace for module inference
        // Reference IDomainEvent from DomusMind.Domain.Abstractions
        [Fact]
        public async Task WriteAsync_WithEmptyCollection_DoesNotAddRows()
        {
            var options = new DbContextOptionsBuilder<DomusMindDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new DomusMindDbContext(options);
            var writer = new EventLogWriter(dbContext);

            await writer.WriteAsync(Array.Empty<DomusMind.Domain.Abstractions.IDomainEvent>(), CancellationToken.None);

            Assert.Empty(dbContext.EventLog);
        }

        [Fact]
        public async Task WriteAsync_WithOneEvent_AddsOneRowWithCorrectValues()
        {
            var occurredAt = DateTime.UtcNow;
            var options = new DbContextOptionsBuilder<DomusMindDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new DomusMindDbContext(options);
            var writer = new EventLogWriter(dbContext);

            var testEvent = new DomusMind.Family.Events.TestFamilyEvent(occurredAt);
            await writer.WriteAsync(new[] { testEvent }, CancellationToken.None);

            var entry = dbContext.EventLog.Single();

            Assert.NotEqual(Guid.Empty, entry.EventId);
            Assert.Equal(nameof(DomusMind.Family.Events.TestFamilyEvent), entry.EventType);
            Assert.Equal("Family", entry.Module);
            Assert.False(string.IsNullOrWhiteSpace(entry.PayloadJson));
            Assert.Equal(occurredAt, entry.OccurredAtUtc);
            Assert.Equal(1, entry.Version);
            Assert.Equal("unknown", entry.AggregateType);
            Assert.Equal("unknown", entry.AggregateId);
        }

        // ...existing code...
    }
}


// Place test event in a namespace containing ".Family.Events" for module inference
namespace DomusMind.Family.Events
{
    public sealed record TestFamilyEvent(DateTime OccurredAtUtc) : DomusMind.Domain.Abstractions.IDomainEvent;
}
