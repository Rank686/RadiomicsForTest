using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MathNet.Numerics.LinearAlgebra;

public abstract class Matrix<T>
{
    public static MatrixBuilder<T> Build { get; } = new MatrixBuilder<T>();

    public virtual int ColumnCount => throw new NotImplementedException();

    public virtual Matrix<T> SubMatrix(int rowIndex, int rowCount, int columnIndex, int columnCount) =>
        throw new NotImplementedException();

    public virtual Matrix<T> PointwiseDivide(Matrix<T> other) => throw new NotImplementedException();

    public virtual Matrix<T> InsertColumn(int columnIndex, Vector<T> column) => throw new NotImplementedException();

    public virtual Vector<T> Column(int index) => throw new NotImplementedException();

    public virtual Evd<T> Evd() => throw new NotImplementedException();

    public static Matrix<T> operator *(Matrix<T> left, Matrix<T> right) => throw new NotImplementedException();
}

public sealed class MatrixBuilder<T>
{
    public Matrix<T> Dense(int rows, int columns, double[] data)
    {
        if (typeof(T) != typeof(double)) throw new NotSupportedException("Only double matrices are supported in the stub implementation.");
        return (Matrix<T>)(object)new DenseMatrixStub(rows, columns, data);
    }

    public Matrix<T> Dense(int rows, int columns, T[] data)
    {
        if (typeof(T) == typeof(double))
        {
            var numeric = data.Cast<object>().Select(Convert.ToDouble).ToArray();
            return (Matrix<T>)(object)new DenseMatrixStub(rows, columns, numeric);
        }
        throw new NotSupportedException("Only double matrices are supported in the stub implementation.");
    }
}

internal sealed class DenseMatrixStub : Matrix<double>
{
    public DenseMatrixStub(int rows, int columns, double[] data)
    {
        RowCount = rows;
        ColumnCountOverride = columns;
        Data = data;
    }

    public int RowCount { get; }
    private int ColumnCountOverride { get; }
    public double[] Data { get; }

    public override int ColumnCount => ColumnCountOverride;
}

public sealed class Vector<T>
{
    public Vector(T[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public T[] Data { get; }

    public int Count => Data.Length;

    public T this[int index] => Data[index];

    public Vector<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var mapped = new TResult[Data.Length];
        for (var i = 0; i < Data.Length; i++)
        {
            mapped[i] = selector(Data[i]);
        }
        return new Vector<TResult>(mapped);
    }

    public T[] ToArray() => (T[])Data.Clone();
}

public sealed class Evd<T>
{
    public Evd(IEnumerable<Complex> eigenValues)
    {
        EigenValues = eigenValues.ToArray();
    }

    public Evd() : this(Array.Empty<Complex>())
    {
    }

    public Complex[] EigenValues { get; }
}
