namespace StockSharp.Quik.Lua
{
	using System;
	using System.IO;
	using System.Security;

	using Ecng.Common;

	using StockSharp.Fix.Dialects;
	using StockSharp.Fix.Native;
	using StockSharp.Messages;

	class LuaDialect : DefaultDialect
	{
		public LuaDialect(string senderCompId, string targetCompId, Stream stream, IncrementalIdGenerator idGenerator, TimeSpan heartbeatInterval, bool isResetCounter, string login, SecureString password, Func<OrderCondition> createOrderCondition)
			: base(senderCompId, targetCompId, stream, idGenerator, heartbeatInterval, isResetCounter, login, password, TimeHelper.Moscow, createOrderCondition)
		{
			SecurityLookup = true;
			PortfolioLookup = true;
			OrderLookup = true;
			SecuritiesSinglePacket = true;
		}

		/// <summary>
		/// Write the specified message into FIX protocol.
		/// </summary>
		/// <param name="writer">The recorder of data in the FIX protocol format.</param>
		/// <param name="message">The message.</param>
		/// <returns><see cref="FixMessages"/> value.</returns>
		protected override string OnWrite(IFixWriter writer, Message message)
		{
			var msgType = base.OnWrite(writer, message);

			var cancelMsg = message as OrderCancelMessage;

			if (cancelMsg == null)
				return msgType;

			if (cancelMsg.OrderType == null)
				return msgType;

			writer.Write(FixTags.Text);
			writer.Write(cancelMsg.OrderType.Value.To<string>());

			return msgType;
		}

		/// <summary>
		/// �������� ������ �� ������� ������.
		/// </summary>
		/// <param name="writer">�������� FIX ������.</param>
		/// <param name="regMsg">���������, ���������� ���������� ��� ����������� ������.</param>
		protected override void WriteOrderCondition(IFixWriter writer, OrderRegisterMessage regMsg)
		{
			writer.WriteOrderCondition((QuikOrderCondition)regMsg.Condition, TimeStampFormat);
		}

		/// <summary>
		/// ��������� ������� ������ <see cref="OrderCondition"/>.
		/// </summary>
		/// <param name="reader">�������� ������.</param>
		/// <param name="tag">���.</param>
		/// <param name="getCondition">�������, ������������ ������� ������.</param>
		/// <returns>������� �� ���������� ������.</returns>
		protected override bool ReadOrderCondition(IFixReader reader, FixTags tag, Func<OrderCondition> getCondition)
		{
			return reader.ReadOrderCondition(tag, TimeZone, TimeStampFormat, () => (QuikOrderCondition)getCondition());
		}
	}
}