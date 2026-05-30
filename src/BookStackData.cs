using System.Text;

namespace BookStackApiClient;

/// <summary>データ補助処理</summary>
internal static class BookStackData
{
    /// <summary>クエリパラメータ構築用拡張メソッド</summary>
    /// <param name="query">クエリパラメータ構築バッファ</param>
    extension(StringBuilder query)
    {
        /// <summary>リスト要求用のパラメータを作成する</summary>
        /// <param name="listing">リスト要求オプション</param>
        /// <returns>クエリパラメータ構築バッファ</returns>
        public StringBuilder AppendQuery(ListingOptions? listing)
        {
            // オプションが無い場合はそのまま返却
            if (listing == null) return query;

            // 取得位置
            if (listing.offset.HasValue)
            {
                if (0 < query.Length) query.Append('&');
                query.Append("offset=").Append(listing.offset.Value);
            }

            // 最大数
            if (listing.count.HasValue)
            {
                if (0 < query.Length) query.Append('&');
                query.Append("count=").Append(listing.count.Value);
            }

            // ソート
            if (listing.sorts != null)
            {
                if (0 < query.Length) query.Append('&');
                var delimiter = "";
                foreach (var sort in listing.sorts)
                {
                    query.Append(delimiter);
                    query.Append("sort=").Append(sort);
                    delimiter = "&";
                }
            }

            // フィルタ
            if (listing.filters != null)
            {
                if (0 < query.Length) query.Append('&');
                var delimiter = "";
                foreach (var filter in listing.filters)
                {
                    query.Append(delimiter);
                    query.Append("filter[").Append(filter.field).Append("]=").Append(filter.expr);
                    delimiter = "&";
                }
            }

            return query;
        }

        /// <summary>検索要求用のパラメータを作成する</summary>
        /// <param name="search">検索オプション</param>
        /// <returns>クエリパラメータ構築バッファ</returns>
        public StringBuilder AppendQuery(SearchArgs? search)
        {
            // オプションが無い場合はそのまま返却
            if (search == null) return query;

            // クエリ文字列
            if (0 < query.Length) query.Append('&');
            query.Append("query=").Append(search.query);

            // ページ番号
            if (search.page.HasValue)
            {
                query.Append("&page=").Append(search.page.Value);
            }

            // 要求数
            if (search.count.HasValue)
            {
                query.Append("&count=").Append(search.count.Value);
            }

            return query;
        }

        /// <summary>検索要求用のパラメータを作成する</summary>
        /// <param name="name">パラメータ名</param>
        /// <param name="value">パラメータ値。null の場合は追加しない。</param>
        /// <returns>クエリパラメータ構築バッファ</returns>
        public StringBuilder AppendParameter(string name, string? value)
        {
            // オプションが無い場合はそのまま返却
            if (value == null) return query;

            // パラメータ追加
            if (0 < query.Length) query.Append('&');
            query.Append(name).Append("=").Append(value);

            return query;
        }

    }
}
