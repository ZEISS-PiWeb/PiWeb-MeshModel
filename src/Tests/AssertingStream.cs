#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    public sealed class AssertingStream : Stream
    {
        private readonly byte[] _Original;
        private readonly MemoryStream _Actual;
        private readonly List<(long start, int count)> _Diffs = new List<(long start, int count)>();
        public AssertingStream( byte[] original )
        {
            _Original = original;
            _Actual = new MemoryStream( _Original.Length );
        }
        public override void Flush()
        {
            _Actual.Flush();
        }
        public override int Read( byte[] buffer, int offset, int count )
        {
            return _Actual.Read( buffer, offset, count );
        }
        public override long Seek( long offset, SeekOrigin origin )
        {
            return _Actual.Seek( offset, origin );
        }
        public override void SetLength( long value )
        {
            _Actual.SetLength( value );
        }
        public override void Write( byte[] buffer, int offset, int count )
        {
            var actual = buffer.AsSpan( offset, count ).ToArray();
            var expected = _Original.AsSpan( (int) Position, count ).ToArray();
            _Actual.Write( buffer, offset, count );
            if( actual.SequenceEqual( expected ) )
            {
                var diffIndex = _Diffs.IndexOf( ( Position - count, count ) );
				if( diffIndex <= -1 )
					return;

				TestContext.WriteLine( $"Correct diff at position: {Position - count} - {Position} (#{diffIndex + 1})" );
				TestContext.WriteLine( $"Corrected: {string.Join( ", ", actual )}" );
				return;
            }
            _Diffs.Add( ( Position - count, count ) );
			TestContext.WriteLine( $"   i    | Expected | Actual  " );
			TestContext.WriteLine( $"--------+----------+----------" );
			const string marker = "<<";
			for( var i = 0; i < expected.Length; ++i )
			{
				var mark = expected[ i ] == actual[ i ] ? string.Empty : marker;
				TestContext.WriteLine( $"{Position - count + i, 7} | {expected[i], 8} |{actual[i], 8}  {mark}" );
			}
        }
        public override bool CanRead => _Actual.CanRead;
        public override bool CanSeek => _Actual.CanSeek;
        public override bool CanWrite => _Actual.CanWrite;
        public override long Length => _Actual.Length;
        public override long Position
        {
            get => _Actual.Position;
            set => _Actual.Position = value;
        }
    }
}