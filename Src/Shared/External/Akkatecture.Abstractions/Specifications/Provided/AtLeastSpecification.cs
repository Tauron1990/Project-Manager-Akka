﻿// The MIT License (MIT)
//
// Copyright (c) 2015-2020 Rasmus Mikkelsen
// Copyright (c) 2015-2020 eBay Software Foundation
// Modified from original source https://github.com/eventflow/EventFlow
//
// Copyright (c) 2018 - 2020 Lutando Ngqakaza
// https://github.com/Lutando/Akkatecture 
// 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Akkatecture.Extensions;
using Tauron.Operations;

namespace Akkatecture.Specifications.Provided;

public class AtLeastSpecification<T> : Specification<T>
{
    private readonly int _requiredSpecifications;
    private readonly IReadOnlyList<ISpecification<T>> _specifications;

    public AtLeastSpecification(
        int requiredSpecifications,
        IEnumerable<ISpecification<T>> specifications)
    {
        var specificationList = specifications.ToList();

        if(requiredSpecifications <= 0)
            throw new ArgumentOutOfRangeException(nameof(requiredSpecifications));
        if(!specificationList.Any())
            throw new ArgumentException("Please provide some specifications", nameof(specifications));
        if(requiredSpecifications > specificationList.Count)
            throw new ArgumentOutOfRangeException(
                $"You required '{requiredSpecifications}' to be met, but only '{specificationList.Count}' was supplied");

        _requiredSpecifications = requiredSpecifications;
        _specifications = specificationList;
    }

    protected override IEnumerable<Error> IsNotSatisfiedBecause(T aggregate)
    {
        var notStatisfiedReasons = _specifications
           .Select(
                specification =>
                (
                    Specification: specification,
                    WhyIsNotStatisfied: specification.WhyIsNotSatisfiedBy(aggregate).ToList()
                ))
           .Where(pair => pair.WhyIsNotStatisfied.Any())
           .Select(
                pair => new Error(
                    $"{pair.Specification.GetType().PrettyPrint()}: {string.Join(", ", pair.WhyIsNotStatisfied.Select(e => e.Info ?? string.Empty))}",
                    string.Join(", ", pair.WhyIsNotStatisfied.Select(e => e.Code))))
           .ToList();

        return _specifications.Count - notStatisfiedReasons.Count >= _requiredSpecifications
            ? Enumerable.Empty<Error>()
            : notStatisfiedReasons;
    }
}