using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectR.Sample.Domain
{
    /// <summary>
    /// A simple Value Object representing a monetary value.
    /// It's immutable.
    /// </summary>
    public record Money(decimal Amount, string Currency);

    /// <summary>
    /// Represents a review for a product. A simple entity.
    /// </summary>
    public class Review
    {
        public Guid Id { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; }
    }

    /// <summary>
    /// Represents a Product entity in our domain.
    /// This is a complex entity designed to showcase ProjectR's features.
    /// It can only be created via a factory method, ensuring it's always in a valid state.
    /// </summary>
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Money Price { get; private set; }
        private readonly List<Review> _reviews = new();
        public IReadOnlyList<Review> Reviews => _reviews.ToList();

        // Private constructor to enforce creation via the factory method.
        private Product() { }

        /// <summary>
        /// The ONLY way to create a valid Product.
        /// ProjectR's default "Build" policy will find and use this method.
        /// </summary>
        public static Product Create(string name, Money? price = null)
        {
            price ??= new Money(0, "USD");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty.", nameof(name));
            if (price.Amount < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            return new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = price
            };
        }

        /// <summary>
        /// A domain method to update the product's name.
        /// </summary>
        public void ChangeName(string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                Name = newName;
            }
        }

        public void AddReview(Review review)
        {
            _reviews.Add(review);
        }
    }
}
